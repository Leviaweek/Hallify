using HallifyDatabase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HallifyDatabase;

public class HallDbContext(DbContextOptions<HallDbContext>  options): DbContext(options)
{
    internal const string PublicSchema = "public";
    
    public DbSet<Hall> Halls { get; set; }
    public DbSet<HallService> HallServices { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingService> BookingServices { get; set; }
}

public class HallDbContextFactory : IDesignTimeDbContextFactory<HallDbContext>
{
    public HallDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HallDbContext>();
        
        const string connectionString = "Server=localhost;Database=HallDb;User Id=leviaweek;Password=dev-password;";
        optionsBuilder.UseNpgsql(connectionString);

        return new HallDbContext(optionsBuilder.Options);
    }
}