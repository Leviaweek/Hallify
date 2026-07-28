using System.Text.Json.Serialization;

namespace HallifyDatabase.Models;

[Serializable]
public sealed record UpdateHallServiceDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("price")] decimal Price);