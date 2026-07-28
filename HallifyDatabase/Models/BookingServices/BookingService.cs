using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HallifyDatabase.Models.HallServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HallifyDatabase.Models.BookingServices;

[Serializable]
[Table("BookingServices",  Schema = HallDbContext.PublicSchema)]
public class BookingService
{
    [Key] public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid HallServiceId { get; set; }
    public HallService HallService { get; set; } = null!;
    public required decimal PriceAtBooking { get; set; }
    public required bool IsDeleted { get; set; }
}

file sealed class BookingServiceConfigure: IEntityTypeConfiguration<BookingService>
{
    public void Configure(EntityTypeBuilder<BookingService> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(bs  => bs.PriceAtBooking)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.HasOne(bs => bs.HallService)
            .WithMany()
            .HasForeignKey(bs => bs.HallServiceId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}