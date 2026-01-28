using Hotel.Application.Abstractions;
using Hotel.Application.Domain;
using Hotel.Application.Dto;
using Hotel.Application.Errors;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Application.Services;

public interface IRoomsService
{
    Task<(IReadOnlyList<RoomDto> Items, int Total)> GetAsync(
        int? minCapacity,
        bool? onlyActive,
        string? type,
        string? sortBy,
        string? sortDir,
        int page,
        int pageSize,
        CancellationToken ct);
    Task<RoomDto> GetByIdAsync(int id, CancellationToken ct);
    Task<RoomDto> CreateAsync(CreateRoomDto dto, CancellationToken ct);
    Task<RoomDto> UpdateAsync(int id, UpdateRoomDto dto, CancellationToken ct);
    Task DeactivateAsync(int id, CancellationToken ct);
}

public sealed class RoomsService(IHotelDbContext db) : IRoomsService
{
    public async Task<(IReadOnlyList<RoomDto> Items, int Total)> GetAsync(
        int? minCapacity,
        bool? onlyActive,
        string? type,
        string? sortBy,
        string? sortDir,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = db.Rooms.AsNoTracking().AsQueryable();

        // 1. Filtrowanie (w bazie)
        if (minCapacity is not null) q = q.Where(r => r.Capacity >= minCapacity);
        if (onlyActive is true) q = q.Where(r => r.IsActive);
        if (!string.IsNullOrWhiteSpace(type))
        {
            var t = type.Trim();
            q = q.Where(r => r.Type == t);
        }

        var total = await q.CountAsync(ct);


        var allFilteredRooms = await q.ToListAsync(ct);


        var dirDesc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        var sortField = (sortBy ?? "number").Trim().ToLowerInvariant();

        var sorted = sortField switch
        {
            "price" or "pricepernight" => dirDesc
                ? allFilteredRooms.OrderByDescending(r => r.PricePerNight).ThenBy(r => r.Number)
                : allFilteredRooms.OrderBy(r => r.PricePerNight).ThenBy(r => r.Number),
            "type" => dirDesc
                ? allFilteredRooms.OrderByDescending(r => r.Type).ThenBy(r => r.Number)
                : allFilteredRooms.OrderBy(r => r.Type).ThenBy(r => r.Number),
            "capacity" => dirDesc
                ? allFilteredRooms.OrderByDescending(r => r.Capacity).ThenBy(r => r.Number)
                : allFilteredRooms.OrderBy(r => r.Capacity).ThenBy(r => r.Number),
            _ => dirDesc
                ? allFilteredRooms.OrderByDescending(r => r.Number)
                : allFilteredRooms.OrderBy(r => r.Number)
        };


        var items = sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RoomDto(r.Id, r.Number, r.Type, r.Capacity, r.PricePerNight, r.IsActive))
            .ToList();

        return (items, total);
    }

    public async Task<RoomDto> GetByIdAsync(int id, CancellationToken ct)
    {
        var r = await db.Rooms.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException($"Room {id} not found");
        return new RoomDto(r.Id, r.Number, r.Type, r.Capacity, r.PricePerNight, r.IsActive);
    }

    public async Task<RoomDto> CreateAsync(CreateRoomDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Number)) throw new ValidationException("Number is required");
        if (dto.Capacity <= 0) throw new ValidationException("Capacity must be > 0");
        if (dto.PricePerNight < 0) throw new ValidationException("PricePerNight must be >= 0");

        var exists = await db.Rooms.AnyAsync(r => r.Number == dto.Number, ct);
        if (exists) throw new ConflictException("Room number must be unique");

        var room = new Room
        {
            Number = dto.Number.Trim(),
            Type = string.IsNullOrWhiteSpace(dto.Type) ? "Standard" : dto.Type.Trim(),
            Capacity = dto.Capacity,
            PricePerNight = dto.PricePerNight,
            IsActive = dto.IsActive
        };

        db.Rooms.Add(room);
        await db.SaveChangesAsync(ct);

        return new RoomDto(room.Id, room.Number, room.Type, room.Capacity, room.PricePerNight, room.IsActive);
    }

    public async Task<RoomDto> UpdateAsync(int id, UpdateRoomDto dto, CancellationToken ct)
    {
        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException($"Room {id} not found");

        if (string.IsNullOrWhiteSpace(dto.Number)) throw new ValidationException("Number is required");
        if (dto.Capacity <= 0) throw new ValidationException("Capacity must be > 0");
        if (dto.PricePerNight < 0) throw new ValidationException("PricePerNight must be >= 0");

        var number = dto.Number.Trim();
        var exists = await db.Rooms.AnyAsync(r => r.Id != id && r.Number == number, ct);
        if (exists) throw new ConflictException("Room number must be unique");

        room.Number = number;
        room.Type = string.IsNullOrWhiteSpace(dto.Type) ? "Standard" : dto.Type.Trim();
        room.Capacity = dto.Capacity;
        room.PricePerNight = dto.PricePerNight;
        room.IsActive = dto.IsActive;

        await db.SaveChangesAsync(ct);

        return new RoomDto(room.Id, room.Number, room.Type, room.Capacity, room.PricePerNight, room.IsActive);
    }

    public async Task DeactivateAsync(int id, CancellationToken ct)
    {
        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException($"Room {id} not found");
        room.IsActive = false;
        await db.SaveChangesAsync(ct);
    }
}