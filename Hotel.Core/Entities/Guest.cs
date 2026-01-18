namespace Hotel.Core.Entities;

public class Guest
{
    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName  { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    
    public string? IdentificationNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    
    // Oskar: Jeden gość może dokonać wielu rezerwacji
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    /// <summary>
    /// Oskar: Metoda pomocnicza zwracajaca pełne imię i nazwisko.
    /// </summary>
    public string GetFullName() => $"{FirstName} {LastName}";
}