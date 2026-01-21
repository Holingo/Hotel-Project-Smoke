using Hotel.Application.Abstractions;
using Hotel.Application.Domain;
using Hotel.Application.Dto;
using Hotel.Application.Errors;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Application.Services;

public interface IGuestsService
{
    Task<(IReadOnlyList<GuestDto> Items, int Total)> GetAsync(int page, int pageSize, CancellationToken ct);
    Task<GuestDto> GetByIdAsync(int id, CancellationToken ct);
    Task<GuestDto> CreateAsync(CreateGuestDto dto, CancellationToken ct);
    Task<GuestDto> UpdateAsync(int id, UpdateGuestDto dto, CancellationToken ct);
}

public sealed class GuestsService(IHotelDbContext db) : IGuestsService
{
    public async Task<(IReadOnlyList<GuestDto> Items, int Total)> GetAsync(int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = db.Guests.AsNoTracking();
        var total = await q.CountAsync(ct);

        var items = await q.OrderBy(g => g.LastName).ThenBy(g => g.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new GuestDto(g.Id, g.FirstName, g.LastName, g.Email, g.Phone, g.IdentityDocument))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<GuestDto> GetByIdAsync(int id, CancellationToken ct)
    {
        var g = await db.Guests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException($"Guest {id} not found");
        return new GuestDto(g.Id, g.FirstName, g.LastName, g.Email, g.Phone, g.IdentityDocument);
    }

    public async Task<GuestDto> CreateAsync(CreateGuestDto dto, CancellationToken ct)
    {
        Validate(dto.FirstName, dto.LastName, dto.Email);

        var g = new Guest
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
            IdentityDocument = string.IsNullOrWhiteSpace(dto.IdentityDocument) ? null : dto.IdentityDocument.Trim()
        };
        db.Guests.Add(g);
        await db.SaveChangesAsync(ct);
        return new GuestDto(g.Id, g.FirstName, g.LastName, g.Email, g.Phone, g.IdentityDocument);
    }

    public async Task<GuestDto> UpdateAsync(int id, UpdateGuestDto dto, CancellationToken ct)
    {
        var g = await db.Guests.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException($"Guest {id} not found");

        Validate(dto.FirstName, dto.LastName, dto.Email);

        g.FirstName = dto.FirstName.Trim();
        g.LastName = dto.LastName.Trim();
        g.Email = dto.Email.Trim();
        g.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        g.IdentityDocument = string.IsNullOrWhiteSpace(dto.IdentityDocument) ? null : dto.IdentityDocument.Trim();

        await db.SaveChangesAsync(ct);
        return new GuestDto(g.Id, g.FirstName, g.LastName, g.Email, g.Phone, g.IdentityDocument);
    }

    private static void Validate(string first, string last, string email)
    {
        if (string.IsNullOrWhiteSpace(first)) throw new ValidationException("FirstName is required");
        if (string.IsNullOrWhiteSpace(last)) throw new ValidationException("LastName is required");
        if (string.IsNullOrWhiteSpace(email)) throw new ValidationException("Email is required");
        if (!email.Contains('@')) throw new ValidationException("Email is invalid");
    }
}
