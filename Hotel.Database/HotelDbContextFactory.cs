using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Hotel.Database;

public class HotelDbContextFactory : IDesignTimeDbContextFactory<HotelDbContext>
{
    public HotelDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HotelDbContext>();

        var connectionString =
            "Server=tcp:student-hotel-project-smoke.database.windows.net,1433;Initial Catalog=hotel-project-smoke;Encrypt=True;TrustServerCertificate=False;Authentication=Active Directory Interactive;";
        
        optionsBuilder.UseSqlServer(connectionString);

        return new HotelDbContext(optionsBuilder.Options);
    }
}