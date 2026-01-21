namespace Hotel.Application.Domain;

public class Reservation
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int GuestId { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public int GuestsCount { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = ReservationStatus.Active;

    // Optimistic concurrency token (bonus). Nullable to avoid provider-specific NOT NULL issues (e.g., SQLite in tests).
    public byte[]? RowVersion { get; set; } = Array.Empty<byte>();

    public Room Room { get; set; } = default!;
    public Guest Guest { get; set; } = default!;
}

public static class ReservationStatus
{
    public const string Active = "Active";
    public const string Canceled = "Canceled";
}
