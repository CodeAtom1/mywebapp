using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Data;
using StackExchange.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppDb(this WebApplicationBuilder builder)
    {
        var mySqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // Add Entity Framework with MySQL
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(
            mySqlConnectionString, ServerVersion.AutoDetect(mySqlConnectionString)
        ));

        return builder.Services;
    }

    public static IServiceCollection AddRedis(this WebApplicationBuilder builder)
    {
        var redisConfig = builder.Configuration["Redis:Configuration"];
        var options = ConfigurationOptions.Parse(redisConfig);
        options.ResolveDns = true;

        if (!string.IsNullOrEmpty(redisConfig))
        {
            var redis = ConnectionMultiplexer.Connect(redisConfig);
            AddEventsToRedis(redis);
            builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
            try
            {
                builder.Services.AddDataProtection()
                    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
                    .SetApplicationName("MyIdentityApp");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DataProtection key storage failed: {ex.Message}");
            }
        }

        // Optional: distributed cache (useful for Session/TempData if you use them)
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConfig;
            options.InstanceName = "MyApp:";
        });
        return builder.Services;
    }
    private static void AddEventsToRedis(ConnectionMultiplexer muxer)
    {
        muxer.ConnectionFailed += (sender, e) =>
        {
            Console.WriteLine($"Redis connection failed: {e.Exception.Message}, Endpoint: {e.EndPoint}");
        };

        muxer.ConnectionRestored += (sender, e) =>
        {
            Console.WriteLine($"Redis connection restored: Endpoint: {e.EndPoint}");
        };

        muxer.ErrorMessage += (sender, e) =>
        {
            Console.WriteLine($"Redis error: {e.Message}");
        };

        muxer.ConfigurationChanged += (sender, e) =>
        {
            Console.WriteLine("Redis configuration changed");
        };

        muxer.HashSlotMoved += (sender, e) =>
        {
            Console.WriteLine("Redis hash slot moved");
        };
    }
    public static IServiceCollection AddIdentityServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.SignIn.RequireConfirmedEmail = false; // change to true if you implement email confirmation
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        return builder.Services;
    }

    public static IServiceCollection AddApplicationCookies(this WebApplicationBuilder builder)
    {
        // Optional: configure cookie options
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = ".MyIdentityApp.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
        });

        return builder.Services;
    }
    public static async Task ConfigureDatabaseSeeding(this WebApplication app)
    {
        // Ensure DB is created/migrated (optional: prefer CI/CD migrations instead)
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
            // Seed roles & admin user
            await DataSeeder.SeedAsync(scope.ServiceProvider);
        }
    }
}
