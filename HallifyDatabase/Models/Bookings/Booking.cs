using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HallifyDatabase.Models.BookingServices;
using HallifyDatabase.Models.Halls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HallifyDatabase.Models;

[Serializable]
[Table("Bookings", Schema = HallDbContext.PublicSchema)]
public class Booking
{
    [Key] public Guid Id { get; set; }

    public required Guid HallId { get; set; }
    public Hall Hall { get; set; } = null!;

    public required DateTimeOffset StartAt { get; set; }
    public required DateTimeOffset EndAt { get; set; }

    public required decimal TotalPrice { get; set; }
    public required bool IsDeleted { get; set; }
    public List<BookingService> BookingServices { get; set; } = [];
}

file sealed class BookingConfigure : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(bs => bs.TotalPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.StartAt).IsRequired();
        builder.Property(x => x.EndAt).IsRequired();

        builder.HasOne(x => x.Hall)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.HallId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(x => x.BookingServices)
            .WithOne()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}