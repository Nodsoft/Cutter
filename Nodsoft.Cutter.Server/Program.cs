using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Nodsoft.Cutter.Server.Components;
using Nodsoft.Cutter.Server.Data;
using Nodsoft.Cutter.Server.Infrastructure.Authorization;
using Nodsoft.Cutter.Server.Infrastructure.Configuration;
using Nodsoft.Cutter.Server.Services;
using OpenIddict.Validation.AspNetCore;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(applyThemeToRedirectedOutput: true)
    .CreateBootstrapLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Logging
builder.Services.AddSerilog(static (services, lc) => lc
    .ReadFrom.Configuration(services.GetRequiredService<IConfiguration>())
    .ReadFrom.Services(services)
);

// API
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

// EF Core: Postgres
builder.Services.AddDbContext<CutterDbContext>(static (services, options) =>
{
    // Add from connection strings
    options.UseNpgsql(services.GetRequiredService<IConfiguration>().GetConnectionString("Database"));
    options.UseOpenIddict<Guid>();
    options.UseSnakeCaseNamingConvention();
});

// Authentication: OpenIddict using GitHub
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<CutterDbContext>()
            .ReplaceDefaultEntities<Guid>();
    })
    .AddClient(options =>
    {
        // Allow the OpenIddict client to negotiate the authorization code flow.
        options.AllowAuthorizationCodeFlow();

        // Register the signing and encryption credentials used to protect
        // sensitive data like the state tokens produced by OpenIddict.
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
            .EnableRedirectionEndpointPassthrough()
            .DisableTransportSecurityRequirement(); // HTTPS is handled by the reverse proxy.

        // Register the GitHub integration.
        options.UseWebProviders()
            .AddGitHub(github => github
                .SetClientId(builder.Configuration["Auth:GitHub:ClientId"] ?? throw new InvalidOperationException("GitHub Client ID not set."))
                .SetClientSecret(builder.Configuration["Auth:GitHub:ClientSecret"] ?? throw new InvalidOperationException("GitHub Client Secret not set."))
                .SetRedirectUri("callback/login/github"));
    });

builder.Services.AddCascadingAuthenticationState();

// Authorization
builder.Services.AddAuthorizationBuilder()
    // Policies
    .AddPolicy(AuthorizationPolicies.AccountEnabled, policy => policy.Requirements.Add(new AccountEnabledRequirement()))
    .AddPolicy(AuthorizationPolicies.OwnLinks, policy => policy.Requirements.Add(new OwnLinksRequirement()));

// Policies
builder.Services.AddScoped<IAuthorizationHandler, AccountEnabledRequirementHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, OwnLinksRequirementHandler>();

// Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LinksService>();

// Configuration
builder.Services.Configure<CutterConfiguration>(builder.Configuration.GetSection("Cutter"));

WebApplication app = builder.Build();

// Request Logging
app.UseSerilogRequestLogging(options =>
{
    // Customize the message template
    options.MessageTemplate = "{RequestScheme} {RequestMethod} {RequestPath} by {RequestClient} responded {StatusCode} in {Elapsed:0.0000} ms";

    // Attach additional properties to the request completion event
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestClient", httpContext.Connection.RemoteIpAddress);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme.ToUpperInvariant());
    };
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();

    IPAddress[] allowedProxies = app.Configuration.GetSection("AllowedProxies").Get<string[]>()?.Select(IPAddress.Parse).ToArray() ?? [];

    // Nginx configuration step
    ForwardedHeadersOptions forwardedHeadersOptions = new()
    {
        ForwardedHeaders = ForwardedHeaders.All
    };

    if (allowedProxies is { Length: not 0 })
    {
        forwardedHeadersOptions.KnownProxies.Clear();

        foreach (IPAddress address in allowedProxies)
        {
            forwardedHeadersOptions.KnownProxies.Add(address);
        }
    }

    app.UseForwardedHeaders(forwardedHeadersOptions);
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapDefaultControllerRoute();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.StartAsync();

await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
{
    // Migrate the database
    await scope.ServiceProvider.GetRequiredService<CutterDbContext>().Database.MigrateAsync();
}

await app.WaitForShutdownAsync();
