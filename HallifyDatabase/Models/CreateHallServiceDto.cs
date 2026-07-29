using System.Text.Json.Serialization;

namespace HallifyDatabase.Models;

[Serializable]
public sealed record CreateHallServiceDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("price")] decimal Price);