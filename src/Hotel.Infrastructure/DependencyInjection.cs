using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        //var cs = cfg.GetConnectionString("HotelDb"); //?? "Data Source=hotel.db";
        services.AddDbContext<HotelDbContext>(options =>
            options.UseSqlServer(
                cfg.GetConnectionString("HotelDb"),
                b => b.MigrationsAssembly("Hotel.Infrastructure")));
        return services;
    }
}