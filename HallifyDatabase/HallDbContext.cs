using HallifyDatabase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HallifyDatabase;

public class HallDbContext(DbContextOptions<HallDbContext>  options): DbContext(options)
{
    internal const string PublicSchema = "public";
    public const string OptionName = "DefaultConnection";
    
    public DbSet<Hall> Halls { get; set; }
    public DbSet<HallService> HallServices { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingService> BookingServices { get; set; }
}