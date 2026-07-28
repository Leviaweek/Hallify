using System.Text.Json.Serialization;

namespace HallifyApi.Queries;

[Serializable]
public sealed record HallSearchQuery(
    [property: JsonPropertyName("startAt")] DateTimeOffset StartAt,
    [property: JsonPropertyName("endAt")] DateTimeOffset EndAt,
    [property: JsonPropertyName("capacity")] int Capacity);