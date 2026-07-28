using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HallifyDatabase.Models;

[Serializable]
[Table("Bookings",  Schema = HallDbContext.PublicSchema)]
public class Booking
{
    [Key] public Guid Id { get; set; }
    
    public Guid HallId { get; set; }

    public Hall Hall { get; set; } = null!;
    
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    
    public decimal TotalPrice { get; set; }
    public List<BookingService> BookingServices { get; set; } = [];
}

file sealed class BookingConfigure: IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(bs  => bs.TotalPrice)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.HasOne(bs => bs.Hall)
            .WithMany()
            .HasForeignKey(bs => bs.HallId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.Property(x => x.StartAt).IsRequired();
        builder.Property(x => x.EndAt).IsRequired();
    }
}