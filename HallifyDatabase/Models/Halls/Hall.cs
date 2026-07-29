using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HallifyDatabase.Models.HallServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HallifyDatabase.Models.Halls;

[Serializable]
[Table("Halls", Schema = HallDbContext.PublicSchema)]
public class Hall
{
    [Key] public Guid Id { get; set; }
    
    [MaxLength(50)]
    public required string Name { get; set; }

    public required int Capacity { get; set; }

    public required decimal HourlyRate { get; set; }

    public List<HallService> HallServices { get; set; } = [];
    public List<Booking> Bookings { get; set; } = [];
    public required bool IsDeleted { get; set; }
}

file sealed class HallConfigure : IEntityTypeConfiguration<Hall>
{
    public void Configure(EntityTypeBuilder<Hall> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.HourlyRate)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasMany(x => x.HallServices)
            .WithOne()
            .HasForeignKey(bs => bs.HallId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasData(
            new Hall
            {
                Id = InitialHalls.HallAId, Name = "Зал А", Capacity = 50, HourlyRate = 2000m, IsDeleted = false
            },
            new Hall
            {
                Id = InitialHalls.HallBId, Name = "Зал B", Capacity = 100, HourlyRate = 3500m, IsDeleted = false
            },
            new Hall { Id = InitialHalls.HallCId, Name = "Зал C", Capacity = 30, HourlyRate = 1500m, IsDeleted = false }
        );
    }
}

internal static class InitialHalls
{
    public static readonly Guid HallAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid HallBId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid HallCId = Guid.Parse("33333333-3333-3333-3333-333333333333");
}