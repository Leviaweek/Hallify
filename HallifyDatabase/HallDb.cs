using System.Data;
using HallifyDatabase.Models;
using HallifyDatabase.Models.BookingServices;
using HallifyDatabase.Models.Halls;
using HallifyDatabase.Models.HallServices;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HallifyDatabase;

public sealed class HallDb(IDbContextFactory<HallDbContext> factory)
{
    public async Task<HallDto?> GetHallAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var hall = await context.Halls
            .AsNoTracking()
            .Where(h => !h.IsDeleted)
            .Where(h => h.Id == id)
            .Select(HallDto.FromHall)
            .FirstOrDefaultAsync(cancellationToken);

        return hall;
    }

    public async Task<Guid?> AddHallAsync(HallDto hall, CancellationToken cancellationToken)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var dbHallServices = hall.HallServices.Select(hs => new HallService
        {
            Name = hs.Name,
            IsDeleted = false,
            Price = hs.Price
        }).ToList();

        var dbHall = new Hall
        {
            Name = hall.Name,
            Capacity = hall.Capacity,
            HourlyRate = hall.HourlyRate,
            IsDeleted = false,
            HallServices = dbHallServices
        };

        context.Halls.Add(dbHall);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return null;
        }

        return dbHall.Id;
    }

    public async Task<bool> DeleteHallAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        try
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            await context.HallServices
                .Where(hs => !hs.IsDeleted)
                .Where(hs => hs.HallId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(hs => hs.IsDeleted, true), cancellationToken);

            var affectedRows = await context.Halls
                .Where(h => !h.IsDeleted)
                .Where(h => h.Id == id)
                .ExecuteUpdateAsync(h => h.SetProperty(x => x.IsDeleted, true), cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return affectedRows == 1;
        }
        catch (NpgsqlException)
        {
            return false;
        }
    }

    public async Task<HallDto[]> GetAvailableHalls(DateTimeOffset startAt, DateTimeOffset endAt, int capacity,
        CancellationToken cancellationToken)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var halls = await context.Halls
            .AsNoTracking()
            .Where(h => !h.IsDeleted)
            .Where(h => h.Capacity >= capacity)
            .Where(h => !h.Bookings
                .Where(b => !b.IsDeleted)
                .Where(b => b.StartAt < endAt)
                .Any(b => b.EndAt > startAt))
            .Select(HallDto.FromHall)
            .ToArrayAsync(cancellationToken);

        return halls;
    }

    public async Task<decimal?> BookHall(Guid id, BookingDto booking, CancellationToken cancellationToken)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var hall = await context.Halls
                .AsNoTracking()
                .Where(h => !h.IsDeleted)
                .Where(h => h.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (hall is null)
                return null;

            var bookingEndAt = booking.StartAt + TimeSpan.FromMinutes(booking.Duration);

            var isOccupied = await context.Bookings
                .Where(b => !b.IsDeleted)
                .Where(b => b.HallId == id)
                .Where(b => b.StartAt < bookingEndAt && b.EndAt > booking.StartAt)
                .AnyAsync(cancellationToken);

            if (isOccupied)
                return null;

            var distinctServices = booking.BookingServices.Distinct().ToList();

            var services = await context.HallServices
                .AsNoTracking()
                .Where(hs => !hs.IsDeleted)
                .Where(hs => hs.HallId == id)
                .Where(hs => distinctServices.Contains(hs.Id))
                .ToDictionaryAsync(hs => hs.Id, cancellationToken);

            if (distinctServices.Count != services.Count)
                return null;

            var bookingServices = distinctServices.Select(hsId => new BookingService
            {
                HallServiceId = services[hsId].Id,
                PriceAtBooking = services[hsId].Price,
                IsDeleted = false
            }).ToList();

            var dbBooking = new Booking
            {
                HallId = hall.Id,
                StartAt = booking.StartAt,
                EndAt = bookingEndAt,
                IsDeleted = false,
                BookingServices = bookingServices,
                TotalPrice = services.Values.Sum(hs => hs.Price) +
                             HallRentalCalculator.CalculateRentalCost(hall.HourlyRate,
                                 booking.StartAt,
                                 bookingEndAt)
            };

            context.Bookings.Add(dbBooking);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return dbBooking.TotalPrice;
        }
        catch (DbUpdateException)
        {
            return null;
        }
    }

    public async Task<HallDto[]> GetAllHalls(CancellationToken cancellationToken)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var halls = await context.Halls
            .Where(h => !h.IsDeleted)
            .Select(HallDto.FromHall)
            .ToArrayAsync(cancellationToken);

        return halls;
    }

    public async Task<bool> UpdateHall(Guid id, UpdateHallDto dto, CancellationToken cancellationToken)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var hall = await context.Halls
            .Include(h => h.HallServices.Where(hs => !hs.IsDeleted))
            .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted, cancellationToken);

        if (hall is null)
            return false;

        hall.Name = dto.Name;
        hall.Capacity = dto.Capacity;
        hall.HourlyRate = dto.HourlyRate;

        var deleteSet = dto.DeleteHallServices.ToHashSet();

        if (deleteSet.Count > 0)
        {
            var servicesToDelete = hall.HallServices
                .Where(hs => deleteSet.Contains(hs.Id))
                .ToList();

            foreach (var service in servicesToDelete) service.IsDeleted = true;
        }

        foreach (var updateDto in dto.UpdateHallServices)
        {
            if (deleteSet.Contains(updateDto.Id))
                continue;

            var existingService = hall.HallServices.FirstOrDefault(hs => hs.Id == updateDto.Id);

            if (existingService is null) continue;

            existingService.Name = updateDto.Name;
            existingService.Price = updateDto.Price;
        }

        foreach (var createDto in dto.CreateHallServices)
            context.HallServices.Add(new HallService
            {
                Id = Guid.NewGuid(),
                HallId = hall.Id,
                Name = createDto.Name,
                Price = createDto.Price,
                IsDeleted = false
            });

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return false;
        }

        return true;
    }
}