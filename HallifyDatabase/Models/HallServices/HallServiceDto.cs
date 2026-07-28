using System.Linq.Expressions;

namespace HallifyDatabase.Models.HallServices;

public sealed record HallServiceDto(Guid Id, string Name, decimal Price)
{
    public static Expression<Func<HallService, HallServiceDto>> FromHallService =>
        hallService => new HallServiceDto(
            hallService.Id,
            hallService.Name,
            hallService.Price);
}