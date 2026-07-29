using System.Text.Json.Serialization;

namespace HallifyDatabase.Models;

[Serializable]
public sealed record UpdateHallDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("capacity")] int Capacity,
    [property: JsonPropertyName("hourlyRate")] decimal HourlyRate,
    [property: JsonPropertyName("createHallServices")] List<CreateHallServiceDto> CreateHallServices,
    [property: JsonPropertyName("updateHallServices")] List<UpdateHallServiceDto> UpdateHallServices,
    [property: JsonPropertyName("deleteHallServices")] List<Guid> DeleteHallServices);   