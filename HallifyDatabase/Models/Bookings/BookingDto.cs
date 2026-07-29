using System.Text.Json.Serialization;

namespace HallifyDatabase.Models;

[Serializable]
public sealed record BookingDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("startAt")] DateTimeOffset StartAt,
    [property: JsonPropertyName("duration")] int Duration,
    [property: JsonPropertyName("bookingServices")] List<Guid> BookingServices);