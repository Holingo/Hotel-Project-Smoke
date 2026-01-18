namespace Hotel.Core.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int GuestId { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public int GuestsCount { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Active"; // Oskar: Active, Canceled

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    
    public Room Room { get; set; } = default!;
    public Guest Guest { get; set; } = default!;

    /// <summary>
    /// Oblicza całkowitą cenę rezerwacji na podstawie liczby dób i ceny pokoju.
    /// </summary>
    public void CalculateTotalPrice(decimal pricePerNight)
    {
        int days = CheckOut.DayNumber - CheckIn.DayNumber;

        if (days < 1) days = 1;
        
        TotalPrice = days * pricePerNight;
    }
}