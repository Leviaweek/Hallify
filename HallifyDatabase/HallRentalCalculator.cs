namespace HallifyDatabase;

public static class HallRentalCalculator
{
    public static decimal CalculateRentalCost(decimal basePrice, DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
            throw new ArgumentException("Час початку не може бути більше ніж кінець");
        if (start.Date != end.Date)
            throw new ArgumentException("Бронювання не може переходити на наступний день");

        var startMinutes = (int)start.TimeOfDay.TotalMinutes;
        var endMinutes = (int)end.TimeOfDay.TotalMinutes;

        decimal total = 0;

        for (var hour = startMinutes / 60; hour < (endMinutes + 59) / 60; hour++)
        {
            var slotStart = Math.Max(hour * 60, startMinutes);
            var slotEnd = Math.Min((hour + 1) * 60, endMinutes);
            var fraction = (slotEnd - slotStart) / 60m;

            var rate = hour switch
            {
                >= 3 and < 6 => 0.9m, // Ранок UTC (06:00 - 09:00 Kyiv)
                >= 9 and < 11 => 1.15m, // Пік UTC  (12:00 - 14:00 Kyiv)
                >= 6 and < 15 => 1.0m, // День UTC  (09:00 - 18:00 Kyiv)
                >= 15 and < 20 => 0.8m, // Вечір UTC (18:00 - 23:00 Kyiv)
                _ => throw new ArgumentOutOfRangeException(nameof(start),
                    "Заклад працює лише з 06:00 до 23:00 по Києву")
            };

            total += basePrice * rate * fraction;
        }

        return total;
    }
}