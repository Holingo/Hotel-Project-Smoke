using Hotel.Application.Abstractions;
using Hotel.Application.Domain;
using Hotel.Application.Dto;
using Hotel.Application.Errors;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Application.Services;

public interface IAvailabilityService
{
    Task<IReadOnlyList<RoomDto>> GetAvailableRoomsAsync(DateOnly checkIn, DateOnly checkOut, int minCapacity, string? type, CancellationToken ct);
}

public sealed class AvailabilityService(IHotelDbContext db) : IAvailabilityService
{
    public async Task<IReadOnlyList<RoomDto>> GetAvailableRoomsAsync(DateOnly checkIn, DateOnly checkOut, int minCapacity, string? type, CancellationToken ct)
    {
        if (checkIn >= checkOut) throw new ValidationException("checkIn must be < checkOut");
        if (minCapacity < 0) minCapacity = 0;

        var unavailableRoomIds = await db.Reservations.AsNoTracking()
            .Where(r => r.Status == ReservationStatus.Active &&
                        r.CheckIn < checkOut && checkIn < r.CheckOut)
            .Select(r => r.RoomId)
            .Distinct()
            .ToListAsync(ct);

        var q = db.Rooms.AsNoTracking().Where(r => r.IsActive && r.Capacity >= minCapacity && !unavailableRoomIds.Contains(r.Id));
        if (!string.IsNullOrWhiteSpace(type))
        {
            var t = type.Trim();
            q = q.Where(r => r.Type == t);
        }

        var rooms = await q
            .OrderBy(r => r.Number)
            .Select(r => new RoomDto(r.Id, r.Number, r.Type, r.Capacity, r.PricePerNight, r.IsActive))
            .ToListAsync(ct);

        return rooms;
    }
}
