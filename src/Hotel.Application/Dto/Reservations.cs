namespace Hotel.Application.Dto;

public record ReservationDto(
    int Id,
    int RoomId,
    int GuestId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int GuestsCount,
    decimal TotalPrice,
    string Status
);

public record CreateReservationDto(
    int RoomId,
    int GuestId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int GuestsCount
);
