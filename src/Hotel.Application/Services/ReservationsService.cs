using Hotel.Application.Abstractions;
using Hotel.Application.Domain;
using Hotel.Application.Dto;
using Hotel.Application.Errors;
using Hotel.Application.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hotel.Application.Services;

public interface IReservationsService
{
    Task<ReservationDto> GetByIdAsync(int id, CancellationToken ct);
    Task<ReservationDto> CreateAsync(CreateReservationDto dto, CancellationToken ct);
    Task CancelAsync(int id, CancellationToken ct);
}

public sealed class ReservationsService(IHotelDbContext db, IOptions<ReservationSettings> settings) : IReservationsService
{
    private readonly ReservationSettings _settings = settings.Value;

    public async Task<ReservationDto> GetByIdAsync(int id, CancellationToken ct)
    {
        var r = await db.Reservations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException($"Reservation {id} not found");

        return Map(r);
    }

    public async Task<ReservationDto> CreateAsync(CreateReservationDto dto, CancellationToken ct)
    {
        if (dto.RoomId <= 0) throw new ValidationException("RoomId must be > 0");
        if (dto.GuestId <= 0) throw new ValidationException("GuestId must be > 0");
        if (dto.GuestsCount <= 0) throw new ValidationException("GuestsCount must be > 0");

        if (dto.CheckIn >= dto.CheckOut) throw new ValidationException("checkIn must be < checkOut");
        var nights = dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;
        if (nights < _settings.MinNights) throw new ValidationException($"Minimum stay is {_settings.MinNights} night(s)");
        if (nights > _settings.MaxNights) throw new ValidationException($"Maximum stay is {_settings.MaxNights} night(s)");

        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Id == dto.RoomId, ct)
            ?? throw new NotFoundException($"Room {dto.RoomId} not found");
        if (!room.IsActive) throw new ValidationException("Room is not active");
        if (dto.GuestsCount > room.Capacity) throw new ValidationException("GuestsCount exceeds room capacity");

        var guestExists = await db.Guests.AnyAsync(g => g.Id == dto.GuestId, ct);
        if (!guestExists) throw new NotFoundException($"Guest {dto.GuestId} not found");

        // Overlap: (existing.CheckIn < new.CheckOut) && (new.CheckIn < existing.CheckOut)
        var hasOverlap = await db.Reservations.AnyAsync(r =>
            r.RoomId == dto.RoomId &&
            r.Status == ReservationStatus.Active &&
            r.CheckIn < dto.CheckOut &&
            dto.CheckIn < r.CheckOut, ct);

        if (hasOverlap) throw new ConflictException("Room already booked in this period");

        var totalPrice = CalculateTotalPrice(dto.CheckIn, dto.CheckOut, room.PricePerNight);

        var reservation = new Reservation
        {
            RoomId = dto.RoomId,
            GuestId = dto.GuestId,
            CheckIn = dto.CheckIn,
            CheckOut = dto.CheckOut,
            GuestsCount = dto.GuestsCount,
            TotalPrice = totalPrice,
            Status = ReservationStatus.Active
        };

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync(ct);

        return Map(reservation);
    }

    public async Task CancelAsync(int id, CancellationToken ct)
    {
        var r = await db.Reservations.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException($"Reservation {id} not found");

        if (r.Status == ReservationStatus.Canceled)
        {
            // idempotent: cancel again -> no content
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (today >= r.CheckIn) throw new ValidationException("Cancellation is allowed only before check-in");

        r.Status = ReservationStatus.Canceled;
        await db.SaveChangesAsync(ct);
    }

    private static ReservationDto Map(Reservation r) =>
        new(r.Id, r.RoomId, r.GuestId, r.CheckIn, r.CheckOut, r.GuestsCount, r.TotalPrice, r.Status);

    private decimal CalculateTotalPrice(DateOnly checkIn, DateOnly checkOut, decimal pricePerNight)
    {
        // Default MVP: nights * pricePerNight.
        // Bonus: weekend surcharge for Fri/Sat nights (configurable).
        decimal total = 0m;
        for (var d = checkIn; d < checkOut; d = d.AddDays(1))
        {
            var multiplier = 1m;
            if (_settings.EnableWeekendSurcharge)
            {
                var dow = d.DayOfWeek;
                if (dow == DayOfWeek.Friday || dow == DayOfWeek.Saturday)
                {
                    multiplier += (_settings.WeekendSurchargePercent / 100m);
                }
            }

            total += pricePerNight * multiplier;
        }

        // Keep the same precision as configured in EF (18,2)
        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }
}
