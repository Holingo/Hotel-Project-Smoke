using Hotel.Infrastructure.Persistence; // Upewnij się, że namespace pasuje
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // 1. Usuwamy standardowy DbContext
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<HotelDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // 2. Tworzymy i otwieramy połączenie SQLite, które TRZYMAMY w pamięci fabryki
            // Używamy Guid, aby każda klasa testowa miała fizycznie inną bazę w RAM
            _connection = new SqliteConnection($"Data Source=InMemory_{Guid.NewGuid()};Mode=Memory;Cache=Shared");
            _connection.Open();

            services.AddDbContext<HotelDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // 3. Dodajemy autoryzację testową
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

            // 4. Budujemy bazę i tabele
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

            db.Database.EnsureCreated(); // To utworzy tabele i uruchomi ziarno (seed)
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        // Zamykamy połączenie dopiero, gdy cała fabryka (zestaw testów) jest usuwana
        _connection?.Close();
        _connection?.Dispose();
    }
}

// Handler autoryzacji (zostaje bez zmian)
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "TestAdmin"), new Claim(ClaimTypes.Role, "Admin") };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}