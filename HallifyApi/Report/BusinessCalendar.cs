using HallifyDatabase;

namespace HallifyApi.Report;

/// <summary>
/// Єдине місце, де зафіксовано робочий календар і тарифну сітку закладу.
/// <see cref="HallRentalCalculator"/> оперує тими самими коефіцієнтами, але у зміщенні UTC;
/// тут вони наведені у локальному (київському) часі, щоб звіти були читабельними для бізнесу.
/// </summary>
public static class BusinessCalendar
{
    /// <summary>Зміщення локального часу закладу відносно UTC.</summary>
    public static readonly TimeSpan LocalOffset = TimeSpan.FromHours(3);

    /// <summary>Перша година, коли заклад приймає бронювання (локальний час).</summary>
    public const int OpenHour = 6;

    /// <summary>Година закриття (локальний час, не включно).</summary>
    public const int CloseHour = 23;

    /// <summary>Кількість годин, які фізично можна продати за одну добу в одному залі.</summary>
    public const int WorkingHoursPerDay = CloseHour - OpenHour;

    public static DateTimeOffset ToLocal(DateTimeOffset value) => value.ToOffset(LocalOffset);

    /// <summary>
    /// Тарифний коефіцієнт для години доби (локальний час).
    /// 0 повертається для неробочих годин — такі бронювання система не створює.
    /// </summary>
    public static decimal HourlyCoefficient(int localHour) => localHour switch
    {
        >= 6 and < 9 => 0.9m,    // ранкова знижка
        >= 9 and < 12 => 1.0m,   // база
        >= 12 and < 14 => 1.15m, // пік
        >= 14 and < 18 => 1.0m,  // база
        >= 18 and < 23 => 0.8m,  // вечірня знижка
        _ => 0m
    };

    /// <summary>Кількість календарних днів у звітному періоді (мінімум 1).</summary>
    public static int DaysInRange(DateTimeOffset from, DateTimeOffset to)
    {
        var days = (ToLocal(to).Date - ToLocal(from).Date).Days;
        return days <= 0 ? 1 : days;
    }

    /// <summary>
    /// Розкладає бронювання на погодинні слоти локального часу.
    /// Логіка дзеркалить <see cref="HallRentalCalculator"/>: частина години рахується дробом.
    /// </summary>
    public static void ForEachHourSlot(DateTimeOffset startAt, DateTimeOffset endAt, Action<int, decimal> onSlot)
    {
        var start = ToLocal(startAt);
        var end = ToLocal(endAt);

        var startMinutes = (int)start.TimeOfDay.TotalMinutes;
        var endMinutes = start.Date == end.Date
            ? (int)end.TimeOfDay.TotalMinutes
            : 24 * 60;

        if (endMinutes <= startMinutes)
            return;

        for (var hour = startMinutes / 60; hour < (endMinutes + 59) / 60; hour++)
        {
            var slotStart = Math.Max(hour * 60, startMinutes);
            var slotEnd = Math.Min((hour + 1) * 60, endMinutes);

            if (slotEnd <= slotStart)
                continue;

            onSlot(hour, (slotEnd - slotStart) / 60m);
        }
    }
}
