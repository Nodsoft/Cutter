using System.Net;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Nodsoft.Cutter.Server.Data.Models;
using OpenIddict.EntityFrameworkCore.Models;

namespace Nodsoft.Cutter.Server.Data;

/// <summary>
/// The database context for the Cutter API.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CutterDbContext(DbContextOptions<CutterDbContext> options) : DbContext(options)
{
    /// <summary>
    /// The set of cutter links in the database.
    /// </summary>
    public DbSet<Link> Links { get; private init; }
    
    /// <summary>
    /// The set of users in the database.
    /// </summary>
    /// <remarks>
    /// This is used to store the users that have authenticated to the API.
    /// </remarks>
    public DbSet<User> Users { get; private init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        UpdateOpenIddictTablesNaming(modelBuilder);
        
        DateTimeOffset y2K = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        modelBuilder.Entity<Link>()
            .Property(l => l.CreatedAt)
            .HasDefaultValueSql(/*lang=sql*/"CURRENT_TIMESTAMP");
        
        modelBuilder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql(/*lang=sql*/"CURRENT_TIMESTAMP");
        
        modelBuilder.Entity<User>()
            .Property(u => u.UpdatedAt)
            .HasDefaultValueSql(/*lang=sql*/"CURRENT_TIMESTAMP");
            
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Username = "__legacy",
            // IpAddresses = [IPAddress.Broadcast],
            CreatedAt = y2K,
            UpdatedAt = y2K,
            RawObject = JsonDocument.Parse("""{"legacy": true}""")
        });

        modelBuilder.Entity<Role>()
            .HasMany<User>()
            .WithMany(u => u.Roles);
        
        modelBuilder.Entity<Role>().HasData(new Role
        {
            Id = 1,
            Name = "admin",
            Description = "Platform administrator, has full access to all management features."
        });
        
        base.OnModelCreating(modelBuilder);
    }
    
    /// <summary>
    /// Updates the OpenIddict tables naming to match Postgres naming conventions.
    /// </summary>
    private static void UpdateOpenIddictTablesNaming(ModelBuilder builder)
    {
        builder.Entity<OpenIddictEntityFrameworkCoreApplication<Guid>>().ToTable("openiddict_applications");
        builder.Entity<OpenIddictEntityFrameworkCoreAuthorization<Guid>>().ToTable("openiddict_authorizations");
        builder.Entity<OpenIddictEntityFrameworkCoreScope<Guid>>().ToTable("openiddict_scopes");
        builder.Entity<OpenIddictEntityFrameworkCoreToken<Guid>>().ToTable("openiddict_tokens");
    }
}