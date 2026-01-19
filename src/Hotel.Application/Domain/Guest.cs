namespace Hotel.Application.Domain;

public class Guest
{
    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Phone { get; set; }
    public string? IdentityDocument { get; set; }

    public List<Reservation> Reservations { get; set; } = new();
}
