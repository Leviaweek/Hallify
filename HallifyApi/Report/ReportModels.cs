namespace HallifyApi.Report;

/// <summary>Межі звітного періоду. Усі звіти беруть бронювання за датою початку.</summary>
public sealed record ReportPeriod(DateTimeOffset From, DateTimeOffset To, int Days);

// ── Звіт 1: завантаженість залів ─────────────────────────────────────────────

public sealed record HallOccupancyRow(
    Guid HallId,
    string HallName,
    int Capacity,
    decimal HourlyRate,
    int Bookings,
    decimal BookedHours,
    decimal AvailableHours,
    decimal UtilizationRate,
    decimal Revenue,
    decimal RevenuePerAvailableHour,
    decimal AverageCheck,
    decimal AverageBookingHours);

public sealed record OccupancyReport(
    ReportPeriod Period,
    decimal TotalUtilizationRate,
    decimal TotalRevenue,
    HallOccupancyRow[] Halls);

// ── Звіт 2: динаміка доходу ──────────────────────────────────────────────────

public sealed record RevenueBucket(
    string Label,
    DateTimeOffset PeriodStart,
    int Bookings,
    decimal RentalRevenue,
    decimal ServicesRevenue,
    decimal TotalRevenue,
    decimal AverageCheck);

public sealed record RevenueReport(
    ReportPeriod Period,
    string Granularity,
    int Bookings,
    decimal RentalRevenue,
    decimal ServicesRevenue,
    decimal TotalRevenue,
    decimal ServicesShare,
    RevenueBucket[] Buckets);

// ── Звіт 3: популярність і дохідність послуг ─────────────────────────────────

public sealed record ServiceUsageRow(
    Guid ServiceId,
    string ServiceName,
    Guid HallId,
    string HallName,
    decimal CurrentPrice,
    bool IsActive,
    int TimesBooked,
    decimal AttachRate,
    decimal Revenue,
    decimal AveragePriceAtBooking);

public sealed record ServicesReport(
    ReportPeriod Period,
    decimal ServicesRevenue,
    ServiceUsageRow[] Services);

// ── Звіт 4: попит по годинах і днях тижня ────────────────────────────────────

public sealed record DemandHourRow(
    int Hour,
    decimal PriceCoefficient,
    int BookingsStarted,
    decimal BookedHours,
    decimal UtilizationRate,
    decimal Revenue);

public sealed record DemandWeekdayRow(
    DayOfWeek Weekday,
    int BookingsStarted,
    decimal BookedHours,
    decimal UtilizationRate,
    decimal Revenue);

public sealed record DemandHeatmapCell(DayOfWeek Weekday, int Hour, decimal BookedHours);

public sealed record DemandReport(
    ReportPeriod Period,
    DemandHourRow[] ByHour,
    DemandWeekdayRow[] ByWeekday,
    DemandHeatmapCell[] Heatmap);

// ── Звіт 5: зведений KPI-дашборд ─────────────────────────────────────────────

public sealed record SummaryReport(
    ReportPeriod Period,
    int Bookings,
    decimal TotalRevenue,
    decimal RentalRevenue,
    decimal ServicesRevenue,
    decimal ServicesShare,
    decimal AverageCheck,
    decimal AverageBookingHours,
    decimal UtilizationRate,
    string? TopHallByRevenue,
    string? TopServiceByRevenue,
    int? BusiestHour,
    DayOfWeek? BusiestWeekday);
