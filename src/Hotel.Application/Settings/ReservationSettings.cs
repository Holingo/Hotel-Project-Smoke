namespace Hotel.Application.Settings;

public sealed class ReservationSettings
{
    public int MaxNights { get; set; } = 30;
    public int MinNights { get; set; } = 1;

    // Bonus pricing rule: weekend surcharge (e.g., 10% more for Fri/Sat nights).
    public bool EnableWeekendSurcharge { get; set; } = true;
    public decimal WeekendSurchargePercent { get; set; } = 10m;
}
