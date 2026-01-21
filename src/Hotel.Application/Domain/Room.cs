namespace Hotel.Application.Domain;

public class Room
{
    public int Id { get; set; }
    public string Number { get; set; } = default!;
    public string Type { get; set; } = "Standard";
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public bool IsActive { get; set; } = true;

    public List<Reservation> Reservations { get; set; } = new();
}
