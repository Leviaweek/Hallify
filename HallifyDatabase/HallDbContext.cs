using HallifyDatabase.Models;
using HallifyDatabase.Models.BookingServices;
using HallifyDatabase.Models.Halls;
using HallifyDatabase.Models.HallServices;
using Microsoft.EntityFrameworkCore;

namespace HallifyDatabase;

public class HallDbContext(DbContextOptions<HallDbContext> options) : DbContext(options)
{
    internal const string PublicSchema = "public";
    public const string OptionName = "DefaultConnection";

    public DbSet<Hall> Halls { get; set; }
    public DbSet<HallService> HallServices { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingService> BookingServices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(PublicSchema);
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HallDbContext).Assembly);
    }
}