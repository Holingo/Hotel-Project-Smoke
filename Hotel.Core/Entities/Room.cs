namespace Hotel.Core.Entities;

public class Room
{
    public int Id { get; set; }
    public string Number { get; set; } = default!;
    public string Type { get; set; } = "Standard";
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Oskar: Tutaj będzie relacja jeden do wielu - pokoj moze miec wiele rezerwacji
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}