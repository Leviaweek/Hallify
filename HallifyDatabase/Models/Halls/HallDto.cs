using System.Linq.Expressions;
using HallifyDatabase.Models.HallServices;

namespace HallifyDatabase.Models.Halls;

public sealed record HallDto(Guid Id, string Name, int Capacity, decimal HourlyRate, List<HallServiceDto> HallServices)
{
    public static Expression<Func<Hall, HallDto>> FromHall =>
        hall => new HallDto(
            hall.Id,
            hall.Name,
            hall.Capacity,
            hall.HourlyRate,
            hall.HallServices.AsQueryable().Where(hs => !hs.IsDeleted)
                .Select(HallServiceDto.FromHallService).ToList());
}