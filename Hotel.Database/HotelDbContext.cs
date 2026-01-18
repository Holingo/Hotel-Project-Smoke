using Hotel.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Database;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasIndex(r => r.Number).IsUnique();
            entity.Property(r => r.PricePerNight).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.Property(r => r.RowVersion).IsRowVersion();
            entity.Property(r => r.TotalPrice).HasPrecision(18, 2);

            entity.HasOne(r => r.Room).WithMany(p => p.Reservations).HasForeignKey(r => r.RoomId);
            entity.HasOne(r => r.Guest).WithMany(g => g.Reservations).HasForeignKey(r => r.GuestId);
        });
        
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>().HasData(
            new Room { Id = 1, Number = "101", Type = "Standard", Capacity = 2, PricePerNight = 200, IsActive = true },
            new Room { Id = 2, Number = "102", Type = "Standard", Capacity = 2, PricePerNight = 200, IsActive = true },
            new Room { Id = 3, Number = "201", Type = "Deluxe", Capacity = 3, PricePerNight = 350, IsActive = true },
            new Room { Id = 4, Number = "301", Type = "Suite", Capacity = 4, PricePerNight = 600, IsActive = true }
        );
        
        modelBuilder.Entity<Guest>().HasData(
            new Guest { Id = 1, FirstName = "Oskar", LastName = "Testowy", Email = "oskar@student.pk.edu.pl" },
            new Guest { Id = 2, FirstName = "Jan", LastName = "Kowalski", Email = "jan.k@example.com" }
        );
    }
}