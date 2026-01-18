using Hotel.Core.Entities;

namespace Hotel.Core.Services;

public interface IReservationService
{
    Task<Reservation> CreateReservationAsync(Reservation reservation);
    
    Task<bool> IsRoomAvailableAsync(int roomId, DateOnly checkIn, DateOnly checkOut);
}