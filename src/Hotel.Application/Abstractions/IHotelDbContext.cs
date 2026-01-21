using Hotel.Application.Domain;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Hotel.Application.Abstractions;

public interface IHotelDbContext
{
    DbSet<Room> Rooms { get; }
    DbSet<Guest> Guests { get; }
    DbSet<Reservation> Reservations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
