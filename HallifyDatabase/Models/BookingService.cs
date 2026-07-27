using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HallifyDatabase.Models;

[Serializable]
[Table("BookingServices",  Schema = HallDbContext.PublicSchema)]
public class BookingService
{
    [Key] public Guid Id { get; set; }
    
    public Guid HallServiceId { get; set; }
    public Guid BookingId { get; set; }

    public HallService HallService { get; set; } = null!;
    public Booking Booking { get; set; } = null!;
    public decimal PriceAtBooking { get; set; }
}

file sealed class BookingServiceConfigure: IEntityTypeConfiguration<BookingService>
{
    public void Configure(EntityTypeBuilder<BookingService> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.HallService)
            .WithMany()
            .HasForeignKey(bs => bs.HallServiceId);
        
        builder.HasOne(x => x.Booking)
            .WithMany(x => x.BookingServices)
            .HasForeignKey(bs => bs.BookingId);
        
        builder.Property(bs  => bs.PriceAtBooking)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}