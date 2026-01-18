using Hotel.Core.Entities;

namespace Hotel.Core.Services;

public class ReservationService
{
    public bool DoesOverlap(DateOnly newStart, DateOnly newEnd, DateOnly existingStart, DateOnly existingEnd)
    {
        return newStart < existingEnd && existingStart < newEnd;
    }

    public bool IsCapacitySufficient(int guestsCount, int roomCapacity)
    {
        return guestsCount <= roomCapacity;
    }

    public bool IsStayDurationValid(DateOnly checkIn, DateOnly checkOut)
    {
        int duration = checkOut.DayNumber - checkIn.DayNumber;
        return duration >= 1 && duration <= 30;
    }
}