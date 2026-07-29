using HallifyDatabase;
using Microsoft.EntityFrameworkCore;

namespace HallifyApi.Report;

/// <summary>
/// Читальний бік системи: агрегати для бізнес-звітів.
/// Свідомо відокремлений від <see cref="HallDb"/> — операційні сценарії й аналітика
/// мають різні вимоги до транзакцій, і змішувати їх в одному класі шкідливо.
/// Жоден метод не змінює стан БД.
/// </summary>
public sealed class ReportsDb(IDbContextFactory<HallDbContext> factory)
{
    // ── Публічні звіти ───────────────────────────────────────────────────────

    public async Task<OccupancyReport> GetOccupancyAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var data = await LoadAsync(from, to, cancellationToken);
        var period = data.Period;
        var availableHours = period.Days * BusinessCalendar.WorkingHoursPerDay;

        var byHall = data.Bookings
            .GroupBy(b => b.HallId)
            .ToDictionary(g => g.Key, g => g.ToArray());

        var rows = data.Halls
            .Select(hall =>
            {
                var bookings = byHall.GetValueOrDefault(hall.Id) ?? [];

                var bookedHours = bookings.Sum(b => b.Hours);
                var revenue = bookings.Sum(b => b.TotalPrice);

                return new HallOccupancyRow(
                    HallId: hall.Id,
                    HallName: hall.Name,
                    Capacity: hall.Capacity,
                    HourlyRate: hall.HourlyRate,
                    Bookings: bookings.Length,
                    BookedHours: Round(bookedHours),
                    AvailableHours: availableHours,
                    UtilizationRate: Rate(bookedHours, availableHours),
                    Revenue: Round(revenue),
                    RevenuePerAvailableHour: Round(Divide(revenue, availableHours)),
                    AverageCheck: Round(Divide(revenue, bookings.Length)),
                    AverageBookingHours: Round(Divide(bookedHours, bookings.Length)));
            })
            .OrderByDescending(r => r.UtilizationRate)
            .ToArray();

        var totalAvailable = availableHours * data.Halls.Length;

        return new OccupancyReport(
            Period: period,
            TotalUtilizationRate: Rate(data.Bookings.Sum(b => b.Hours), totalAvailable),
            TotalRevenue: Round(data.Bookings.Sum(b => b.TotalPrice)),
            Halls: rows);
    }

    public async Task<RevenueReport> GetRevenueAsync(
        DateTimeOffset from, DateTimeOffset to, RevenueGranularity granularity, CancellationToken cancellationToken)
    {
        var data = await LoadAsync(from, to, cancellationToken);

        var buckets = data.Bookings
            .GroupBy(b => BucketStart(b.StartAt, granularity))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var total = g.Sum(b => b.TotalPrice);
                var services = g.Sum(b => b.ServicesTotal);

                return new RevenueBucket(
                    Label: BucketLabel(g.Key, granularity),
                    PeriodStart: g.Key,
                    Bookings: g.Count(),
                    RentalRevenue: Round(total - services),
                    ServicesRevenue: Round(services),
                    TotalRevenue: Round(total),
                    AverageCheck: Round(Divide(total, g.Count())));
            })
            .ToArray();

        var totalRevenue = data.Bookings.Sum(b => b.TotalPrice);
        var servicesRevenue = data.Bookings.Sum(b => b.ServicesTotal);

        return new RevenueReport(
            Period: data.Period,
            Granularity: granularity.ToString().ToLowerInvariant(),
            Bookings: data.Bookings.Length,
            RentalRevenue: Round(totalRevenue - servicesRevenue),
            ServicesRevenue: Round(servicesRevenue),
            TotalRevenue: Round(totalRevenue),
            ServicesShare: Rate(servicesRevenue, totalRevenue),
            Buckets: buckets);
    }

    public async Task<ServicesReport> GetServicesAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var data = await LoadAsync(from, to, cancellationToken);

        var bookingsPerHall = data.Bookings
            .GroupBy(b => b.HallId)
            .ToDictionary(g => g.Key, g => g.Count());

        var lines = data.Bookings
            .SelectMany(b => b.Services)
            .GroupBy(s => s.HallServiceId)
            .ToDictionary(g => g.Key, g => g.ToArray());

        var rows = data.Services.Values
            .Select(service =>
            {
                var usage = lines.GetValueOrDefault(service.Id) ?? [];
                var hallBookings = bookingsPerHall.GetValueOrDefault(service.HallId);
                var revenue = usage.Sum(u => u.PriceAtBooking);

                return new ServiceUsageRow(
                    ServiceId: service.Id,
                    ServiceName: service.Name,
                    HallId: service.HallId,
                    HallName: data.HallNames.GetValueOrDefault(service.HallId) ?? "—",
                    CurrentPrice: service.Price,
                    IsActive: !service.IsDeleted,
                    TimesBooked: usage.Length,
                    AttachRate: Rate(usage.Length, hallBookings),
                    Revenue: Round(revenue),
                    AveragePriceAtBooking: Round(Divide(revenue, usage.Length)));
            })
            .Where(r => r.IsActive || r.TimesBooked > 0)
            .OrderByDescending(r => r.Revenue)
            .ToArray();

        return new ServicesReport(
            Period: data.Period,
            ServicesRevenue: Round(data.Bookings.Sum(b => b.ServicesTotal)),
            Services: rows);
    }

    public async Task<DemandReport> GetDemandAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var data = await LoadAsync(from, to, cancellationToken);

        var hourHours = new Dictionary<int, decimal>();
        var hourStarts = new Dictionary<int, int>();
        var hourRevenue = new Dictionary<int, decimal>();

        var weekdayHours = new Dictionary<DayOfWeek, decimal>();
        var weekdayStarts = new Dictionary<DayOfWeek, int>();
        var weekdayRevenue = new Dictionary<DayOfWeek, decimal>();

        var heatmap = new Dictionary<(DayOfWeek Weekday, int Hour), decimal>();

        foreach (var booking in data.Bookings)
        {
            var local = BusinessCalendar.ToLocal(booking.StartAt);
            var weekday = local.DayOfWeek;
            var totalHours = booking.Hours;

            Increment(hourStarts, local.Hour, 1);
            Increment(weekdayStarts, weekday, 1);
            Increment(weekdayHours, weekday, totalHours);
            Increment(weekdayRevenue, weekday, booking.TotalPrice);

            BusinessCalendar.ForEachHourSlot(booking.StartAt, booking.EndAt, (hour, fraction) =>
            {
                Increment(hourHours, hour, fraction);
                Increment(heatmap, (weekday, hour), fraction);

                // дохід розподіляється пропорційно фактично зайнятим годинам
                if (totalHours > 0)
                    Increment(hourRevenue, hour, booking.TotalPrice * (fraction / totalHours));
            });
        }

        var byHour = Enumerable
            .Range(BusinessCalendar.OpenHour, BusinessCalendar.WorkingHoursPerDay)
            .Select(hour =>
            {
                var hours = hourHours.GetValueOrDefault(hour);

                return new DemandHourRow(
                    Hour: hour,
                    PriceCoefficient: BusinessCalendar.HourlyCoefficient(hour),
                    BookingsStarted: hourStarts.GetValueOrDefault(hour),
                    BookedHours: Round(hours),
                    UtilizationRate: Rate(hours, data.Period.Days * data.Halls.Length),
                    Revenue: Round(hourRevenue.GetValueOrDefault(hour)));
            })
            .ToArray();

        var weekdayCapacity = Divide(data.Period.Days, 7) * BusinessCalendar.WorkingHoursPerDay * data.Halls.Length;

        var byWeekday = Enum.GetValues<DayOfWeek>()
            .Select(weekday => new DemandWeekdayRow(
                Weekday: weekday,
                BookingsStarted: weekdayStarts.GetValueOrDefault(weekday),
                BookedHours: Round(weekdayHours.GetValueOrDefault(weekday)),
                UtilizationRate: Rate(weekdayHours.GetValueOrDefault(weekday), weekdayCapacity),
                Revenue: Round(weekdayRevenue.GetValueOrDefault(weekday))))
            .ToArray();

        var cells = heatmap
            .OrderBy(cell => cell.Key.Weekday)
            .ThenBy(cell => cell.Key.Hour)
            .Select(cell => new DemandHeatmapCell(cell.Key.Weekday, cell.Key.Hour, Round(cell.Value)))
            .ToArray();

        return new DemandReport(data.Period, byHour, byWeekday, cells);
    }

    public async Task<SummaryReport> GetSummaryAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var occupancy = await GetOccupancyAsync(from, to, cancellationToken);
        var revenue = await GetRevenueAsync(from, to, RevenueGranularity.Day, cancellationToken);
        var services = await GetServicesAsync(from, to, cancellationToken);
        var demand = await GetDemandAsync(from, to, cancellationToken);

        var bookedHours = occupancy.Halls.Sum(h => h.BookedHours);

        var topHour = demand.ByHour
            .Where(h => h.BookedHours > 0)
            .OrderByDescending(h => h.BookedHours)
            .FirstOrDefault();

        var topWeekday = demand.ByWeekday
            .Where(w => w.BookedHours > 0)
            .OrderByDescending(w => w.BookedHours)
            .FirstOrDefault();

        return new SummaryReport(
            Period: occupancy.Period,
            Bookings: revenue.Bookings,
            TotalRevenue: revenue.TotalRevenue,
            RentalRevenue: revenue.RentalRevenue,
            ServicesRevenue: revenue.ServicesRevenue,
            ServicesShare: revenue.ServicesShare,
            AverageCheck: Round(Divide(revenue.TotalRevenue, revenue.Bookings)),
            AverageBookingHours: Round(Divide(bookedHours, revenue.Bookings)),
            UtilizationRate: occupancy.TotalUtilizationRate,
            TopHallByRevenue: occupancy.Halls.OrderByDescending(h => h.Revenue).FirstOrDefault(h => h.Revenue > 0)?.HallName,
            TopServiceByRevenue: services.Services.FirstOrDefault(s => s.Revenue > 0)?.ServiceName,
            BusiestHour: topHour?.Hour,
            BusiestWeekday: topWeekday?.Weekday);
    }

    // ── Завантаження даних ───────────────────────────────────────────────────

    /// <summary>
    /// Один похід у БД на звіт: пласка проєкція бронювань періоду + довідники залів і послуг.
    /// Обсяги бронювань одного закладу за звітний період вимірюються тисячами рядків,
    /// тому агрегація в пам'яті лишається дешевшою за складні SQL-групування,
    /// а код звітів — читабельним. Для більших обсягів ці ж формули переносяться
    /// у матеріалізований вигляд без зміни контракту API.
    /// </summary>
    private async Task<ReportDataset> LoadAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var rawHalls = await context.Halls
            .AsNoTracking()
            .Where(h => !h.IsDeleted)
            .Select(h => new { h.Id, h.Name, h.Capacity, h.HourlyRate })
            .ToArrayAsync(cancellationToken);

        var rawServices = await context.HallServices
            .AsNoTracking()
            .Select(hs => new { hs.Id, hs.HallId, hs.Name, hs.Price, hs.IsDeleted })
            .ToArrayAsync(cancellationToken);

        var halls = rawHalls
            .Select(h => new HallRow(h.Id, h.Name, h.Capacity, h.HourlyRate))
            .ToArray();

        var services = rawServices
            .Select(hs => new ServiceRow(hs.Id, hs.HallId, hs.Name, hs.Price, hs.IsDeleted))
            .ToArray();

        var raw = await context.Bookings
            .AsNoTracking()
            .Where(b => !b.IsDeleted)
            .Where(b => b.StartAt >= from && b.StartAt < to)
            .Select(b => new
            {
                b.HallId,
                b.StartAt,
                b.EndAt,
                b.TotalPrice,
                Services = b.BookingServices
                    .Where(bs => !bs.IsDeleted)
                    .Select(bs => new { bs.HallServiceId, bs.PriceAtBooking })
                    .ToList()
            })
            .ToArrayAsync(cancellationToken);

        var bookings = raw
            .Select(b => new BookingRow(
                b.HallId,
                b.StartAt,
                b.EndAt,
                b.TotalPrice,
                b.Services.Select(s => new BookingServiceRow(s.HallServiceId, s.PriceAtBooking)).ToArray()))
            .ToArray();

        return new ReportDataset(
            Period: new ReportPeriod(from, to, BusinessCalendar.DaysInRange(from, to)),
            Halls: halls,
            HallNames: halls.ToDictionary(h => h.Id, h => h.Name),
            Services: services.ToDictionary(s => s.Id),
            Bookings: bookings);
    }

    // ── Допоміжні функції ────────────────────────────────────────────────────

    private static DateTimeOffset BucketStart(DateTimeOffset value, RevenueGranularity granularity)
    {
        var local = BusinessCalendar.ToLocal(value);

        var date = granularity switch
        {
            RevenueGranularity.Week => local.Date.AddDays(-(((int)local.DayOfWeek + 6) % 7)),
            RevenueGranularity.Month => new DateTime(local.Year, local.Month, 1),
            _ => local.Date
        };

        return new DateTimeOffset(date, BusinessCalendar.LocalOffset);
    }

    private static string BucketLabel(DateTimeOffset start, RevenueGranularity granularity) => granularity switch
    {
        RevenueGranularity.Week => $"{start:yyyy-MM-dd} — {start.AddDays(6):yyyy-MM-dd}",
        RevenueGranularity.Month => start.ToString("yyyy-MM"),
        _ => start.ToString("yyyy-MM-dd")
    };

    private static void Increment<TKey>(Dictionary<TKey, decimal> target, TKey key, decimal value)
        where TKey : notnull =>
        target[key] = target.GetValueOrDefault(key) + value;

    private static void Increment<TKey>(Dictionary<TKey, int> target, TKey key, int value)
        where TKey : notnull =>
        target[key] = target.GetValueOrDefault(key) + value;

    private static decimal Divide(decimal numerator, decimal denominator) =>
        denominator == 0 ? 0 : numerator / denominator;

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal Rate(decimal numerator, decimal denominator) =>
        Math.Round(Divide(numerator, denominator), 4, MidpointRounding.AwayFromZero);

    // ── Внутрішні моделі читання ─────────────────────────────────────────────

    private sealed record ReportDataset(
        ReportPeriod Period,
        HallRow[] Halls,
        Dictionary<Guid, string> HallNames,
        Dictionary<Guid, ServiceRow> Services,
        BookingRow[] Bookings);

    private sealed record HallRow(Guid Id, string Name, int Capacity, decimal HourlyRate);

    private sealed record ServiceRow(Guid Id, Guid HallId, string Name, decimal Price, bool IsDeleted);

    private sealed record BookingServiceRow(Guid HallServiceId, decimal PriceAtBooking);

    private sealed record BookingRow(
        Guid HallId,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt,
        decimal TotalPrice,
        BookingServiceRow[] Services)
    {
        public decimal ServicesTotal => Services.Sum(s => s.PriceAtBooking);

        public decimal Hours => (decimal)(EndAt - StartAt).TotalHours;
    }
}

public enum RevenueGranularity
{
    Day,
    Week,
    Month
}
