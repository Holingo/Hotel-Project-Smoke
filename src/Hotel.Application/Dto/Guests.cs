namespace Hotel.Application.Dto;

public record GuestDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? IdentityDocument
);

public record CreateGuestDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? IdentityDocument
);

public record UpdateGuestDto(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? IdentityDocument
);
