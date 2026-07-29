using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HallifyApi.Report;

/// <summary>
/// Аналітичний контур API. Усі ендпоінти — тільки читання, спільний контракт періоду:
/// <c>?from=2026-07-01T00:00:00%2B03:00&amp;to=2026-08-01T00:00:00%2B03:00</c>.
/// Якщо період не переданий — беруться останні 30 днів.
/// </summary>
public static class ReportEndpoints
{
    private const int DefaultRangeDays = 30;
    private const int MaxRangeDays = 366;

    public static IEndpointRouteBuilder MapReports(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports");

        group.MapGet("/summary", async Task<Results<Ok<SummaryReport>, BadRequest<string>>>
            ([AsParameters] ReportRangeQuery query, [FromServices] ReportsDb db, CancellationToken cancellationToken) =>
        {
            if (!TryResolveRange(query, out var range, out var error))
                return TypedResults.BadRequest(error);

            return TypedResults.Ok(await db.GetSummaryAsync(range.From, range.To, cancellationToken));
        });

        group.MapGet("/occupancy", async Task<Results<Ok<OccupancyReport>, BadRequest<string>>>
            ([AsParameters] ReportRangeQuery query, [FromServices] ReportsDb db, CancellationToken cancellationToken) =>
        {
            if (!TryResolveRange(query, out var range, out var error))
                return TypedResults.BadRequest(error);

            return TypedResults.Ok(await db.GetOccupancyAsync(range.From, range.To, cancellationToken));
        });

        group.MapGet("/revenue", async Task<Results<Ok<RevenueReport>, BadRequest<string>>>
            ([AsParameters] ReportRangeQuery query, [FromQuery] string? granularity, [FromServices] ReportsDb db,
                CancellationToken cancellationToken) =>
        {
            if (!TryResolveRange(query, out var range, out var error))
                return TypedResults.BadRequest(error);

            if (!TryParseGranularity(granularity, out var parsed))
                return TypedResults.BadRequest("granularity має бути day, week або month");

            return TypedResults.Ok(await db.GetRevenueAsync(range.From, range.To, parsed, cancellationToken));
        });

        group.MapGet("/services", async Task<Results<Ok<ServicesReport>, BadRequest<string>>>
            ([AsParameters] ReportRangeQuery query, [FromServices] ReportsDb db, CancellationToken cancellationToken) =>
        {
            if (!TryResolveRange(query, out var range, out var error))
                return TypedResults.BadRequest(error);

            return TypedResults.Ok(await db.GetServicesAsync(range.From, range.To, cancellationToken));
        });

        group.MapGet("/demand", async Task<Results<Ok<DemandReport>, BadRequest<string>>>
            ([AsParameters] ReportRangeQuery query, [FromServices] ReportsDb db, CancellationToken cancellationToken) =>
        {
            if (!TryResolveRange(query, out var range, out var error))
                return TypedResults.BadRequest(error);

            return TypedResults.Ok(await db.GetDemandAsync(range.From, range.To, cancellationToken));
        });

        return app;
    }

    private static bool TryResolveRange(
        ReportRangeQuery query,
        out (DateTimeOffset From, DateTimeOffset To) range,
        out string error)
    {
        var to = query.To ?? DateTimeOffset.UtcNow;
        var from = query.From ?? to.AddDays(-DefaultRangeDays);

        range = (from, to);
        error = string.Empty;

        if (from >= to)
        {
            error = "from має бути раніше за to";
            return false;
        }

        if ((to - from).TotalDays > MaxRangeDays)
        {
            error = $"звітний період не може перевищувати {MaxRangeDays} днів";
            return false;
        }

        return true;
    }

    private static bool TryParseGranularity(string? value, out RevenueGranularity granularity)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            granularity = RevenueGranularity.Day;
            return true;
        }

        return Enum.TryParse(value, ignoreCase: true, out granularity)
               && Enum.IsDefined(granularity);
    }
}

public sealed record ReportRangeQuery(DateTimeOffset? From, DateTimeOffset? To);
