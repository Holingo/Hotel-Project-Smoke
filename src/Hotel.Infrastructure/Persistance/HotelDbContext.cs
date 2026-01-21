using Hotel.Application.Abstractions;
using Hotel.Application.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Hotel.Infrastructure.Persistence;

public class HotelDbContext(DbContextOptions<HotelDbContext> options) : DbContext(options), IHotelDbContext
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Room>(e =>
        {
            e.HasIndex(x => x.Number).IsUnique();
            e.Property(x => x.Number).HasMaxLength(32).IsRequired();
            e.Property(x => x.Type).HasMaxLength(32).IsRequired();
        });

        b.Entity<Guest>(e =>
        {
            e.Property(x => x.FirstName).HasMaxLength(64).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(64).IsRequired();
            e.Property(x => x.Email).HasMaxLength(128).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(32);
            e.Property(x => x.IdentityDocument).HasMaxLength(64);
            e.HasIndex(x => x.Email);
        });

        // DateOnly -> string (ISO) converter for broad provider compatibility
        var dateOnlyConverter = new ValueConverter<DateOnly, string>(
            d => d.ToString("yyyy-MM-dd"),
            s => DateOnly.Parse(s));

        b.Entity<Reservation>(e =>
        {
            e.Property(x => x.CheckIn).HasConversion(dateOnlyConverter).HasMaxLength(10);
            e.Property(x => x.CheckOut).HasConversion(dateOnlyConverter).HasMaxLength(10);
            e.Property(x => x.Status).HasMaxLength(16).IsRequired();
            e.Property(x => x.TotalPrice).HasPrecision(18, 2);
            e.Property(x => x.RowVersion)
                .IsRowVersion()
                .IsRequired(false);

            e.HasOne(x => x.Room).WithMany(r => r.Reservations).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Guest).WithMany(g => g.Reservations).HasForeignKey(x => x.GuestId).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.RoomId, x.CheckIn, x.CheckOut });
        });

        // Seed (optional but useful)
        b.Entity<Room>().HasData(
            new Room { Id = 1, Number = "101", Type = "Standard", Capacity = 2, PricePerNight = 250, IsActive = true },
            new Room { Id = 2, Number = "102", Type = "Standard", Capacity = 3, PricePerNight = 320, IsActive = true },
            new Room { Id = 3, Number = "201", Type = "Deluxe", Capacity = 2, PricePerNight = 450, IsActive = true },
            new Room { Id = 4, Number = "202", Type = "Deluxe", Capacity = 4, PricePerNight = 600, IsActive = true },
            new Room { Id = 5, Number = "301", Type = "Suite", Capacity = 4, PricePerNight = 900, IsActive = true },
            new Room { Id = 6, Number = "999", Type = "Maintenance", Capacity = 1, PricePerNight = 0, IsActive = false }
        );

        b.Entity<Guest>().HasData(
            new Guest { Id = 1, FirstName = "Jan", LastName = "Kowalski", Email = "jan.kowalski@example.com", Phone = "500600700", IdentityDocument = "ABC123456" },
            new Guest { Id = 2, FirstName = "Anna", LastName = "Nowak", Email = "anna.nowak@example.com", Phone = null, IdentityDocument = null },
            new Guest { Id = 3, FirstName = "Piotr", LastName = "Zielinski", Email = "piotr.zielinski@example.com", Phone = "123123123", IdentityDocument = "XYZ987654" }
        );

        // One example future reservation to test availability
        b.Entity<Reservation>().HasData(
            new Reservation
            {
                Id = 1,
                RoomId = 1,
                GuestId = 1,
                CheckIn = new DateOnly(2030, 1, 10),
                CheckOut = new DateOnly(2030, 1, 12),
                GuestsCount = 2,
                TotalPrice = 2 * 250m,
                Status = ReservationStatus.Active,
                RowVersion = Array.Empty<byte>()
            }
        );
    }
}
