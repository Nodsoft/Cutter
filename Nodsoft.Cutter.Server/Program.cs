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

namespace Nodsoft.Cutter.Server;

public class Program
{
    private static ConfigurationManager _configuration = null!;
    
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(applyThemeToRedirectedOutput: true)
            .CreateBootstrapLogger();
        
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        _configuration = builder.Configuration;
        
        // Add services to the container.
        ConfigureServices(builder.Services);

        WebApplication app = builder.Build();
        Configure(app);

        await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
        {
            // Migrate the database
            await scope.ServiceProvider.GetRequiredService<CutterDbContext>().Database.MigrateAsync();
        }
        
        await app.RunAsync();
    }
    
    /// <summary>
    /// Adds services to the application's service collection.
    /// </summary>
    /// <param name="services">The services collection to add services to.</param>
    /// <returns>The updated services collection.</returns>
    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddSerilog(static (services, lc) => lc
            .ReadFrom.Configuration(services.GetRequiredService<IConfiguration>())
            .ReadFrom.Services(services)
        );
        
        // API
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
        
        // Blazor
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddHttpContextAccessor();
        
        // EF Core: Postgres
        services.AddDbContext<CutterDbContext>(static (services, options) =>
        {
            // Add from connection strings
            options.UseNpgsql(services.GetRequiredService<IConfiguration>().GetConnectionString("Database"));
            options.UseOpenIddict<Guid>();
            options.UseSnakeCaseNamingConvention();
        });
        
        // Authentication: OpenIddict using GitHub
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie();
        
        services.AddOpenIddict()
            .AddCore(static options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<CutterDbContext>()
                    .ReplaceDefaultEntities<Guid>();
            })
            .AddClient(static options =>
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
                    .AddGitHub(static options => options
                        .SetClientId(_configuration["Auth:GitHub:ClientId"] ?? throw new InvalidOperationException("GitHub Client ID not set."))
                        .SetClientSecret(_configuration["Auth:GitHub:ClientSecret"] ?? throw new InvalidOperationException("GitHub Client Secret not set."))
                        .SetRedirectUri("callback/login/github"));
            });

        services.AddCascadingAuthenticationState();
        
        // Authorization
        services.AddAuthorizationBuilder()
            // Policies
            .AddPolicy(AuthorizationPolicies.AccountEnabled, policy => policy.Requirements.Add(new AccountEnabledRequirement()))
            .AddPolicy(AuthorizationPolicies.OwnLinks, policy => policy.Requirements.Add(new OwnLinksRequirement()));
        
        // Policies
        services.AddScoped<IAuthorizationHandler, AccountEnabledRequirementHandler>();
        services.AddSingleton<IAuthorizationHandler, OwnLinksRequirementHandler>();
            
        // Services
        services.AddScoped<UserService>();
        services.AddScoped<LinksService>();
        
        // Configuration
        services.Configure<CutterConfiguration>(_configuration.GetSection("Cutter"));
        
        return services;
    }
    
    /// <summary>
    /// Configures the application's request pipeline.
    /// </summary>
    /// <param name="app">The application to configure.</param>
    /// <returns>The updated application.</returns>
    private static WebApplication Configure(WebApplication app)
    {
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
        
        return app;
    }
}