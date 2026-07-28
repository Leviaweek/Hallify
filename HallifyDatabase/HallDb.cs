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
            .AsSingleQuery()
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
            Price = hs.Price,
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
            .AsSingleQuery()
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

    public async Task<decimal?> BookHall(BookingDto booking, CancellationToken cancellationToken)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);
        
        var hall = await context.Halls
            .AsSingleQuery()
            .Where(h => !h.IsDeleted)
            .Where(h => h.Id == booking.HallId)
            .FirstOrDefaultAsync(cancellationToken);

        if (hall is null)
            return null;

        var distinctServices =  booking.BookingServices.Distinct().ToList();

        var services = await context.HallServices
            .AsSingleQuery()
            .Where(hs => !hs.IsDeleted)
            .Where(hs => hs.HallId == booking.HallId)
            .Where(hs => distinctServices.Contains(hs.Id))
            .ToDictionaryAsync(hs => hs.Id, cancellationToken);

        if (distinctServices.Count != services.Count)
            throw new ArgumentException("Деякі послуги не знайдені");

        var bookingServices = distinctServices.Select(hs => new BookingService
        {
            HallServiceId = services[hs].Id,
            PriceAtBooking = services[hs].Price,
            HallService = services[hs],
            IsDeleted = false,
            
        }).ToList();

        var bookingEndAt = booking.StartAt + TimeSpan.FromMinutes(booking.Duration);
        
        var dbBooking = new Booking
        {
            Hall = hall,
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

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return null;
        }
        
        return dbBooking.TotalPrice;
    }
    
    public async Task<bool> UpdateHall(UpdateHallDto dto, CancellationToken cancellationToken)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var hall = await context.Halls
            .Include(h => h.HallServices.Where(hs => !hs.IsDeleted))
            .FirstOrDefaultAsync(h => h.Id == dto.Id && !h.IsDeleted, cancellationToken);

        if (hall is null)
            return false;

        hall.Name = dto.Name;
        hall.Capacity = dto.Capacity;
        hall.HourlyRate = dto.HourlyRate;

        if (dto.DeleteHallServices.Count > 0)
        {
            var servicesToDelete = hall.HallServices
                .Where(hs => dto.DeleteHallServices.Contains(hs.Id))
                .ToList();

            foreach (var service in servicesToDelete)
            {
                service.IsDeleted = true;
            }
        }

        var deleteSet = dto.DeleteHallServices.ToHashSet();
    
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
        {
            hall.HallServices.Add(new HallService
            {
                Id = Guid.NewGuid(),
                HallId = hall.Id,
                Name = createDto.Name,
                Price = createDto.Price,
                IsDeleted = false
            });
        }

        await context.SaveChangesAsync(cancellationToken);

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