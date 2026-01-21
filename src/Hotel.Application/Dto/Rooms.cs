namespace Hotel.Application.Dto;

public record RoomDto(
    int Id,
    string Number,
    string Type,
    int Capacity,
    decimal PricePerNight,
    bool IsActive
);

public record CreateRoomDto(
    string Number,
    string Type,
    int Capacity,
    decimal PricePerNight,
    bool IsActive = true
);

public record UpdateRoomDto(
    string Number,
    string Type,
    int Capacity,
    decimal PricePerNight,
    bool IsActive
);
