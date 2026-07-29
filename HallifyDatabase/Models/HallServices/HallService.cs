using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HallifyDatabase.Models.Halls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HallifyDatabase.Models.HallServices;

[Serializable]
[Table("HallServices", Schema = HallDbContext.PublicSchema)]
public class HallService
{
    [Key] public Guid Id { get; set; }

    public Guid HallId { get; set; }

    [MaxLength(50)] public required string Name { get; set; }

    public required decimal Price { get; set; }
    public required bool IsDeleted { get; set; }
}

file sealed class HallServiceConfigure : IEntityTypeConfiguration<HallService>
{
    public void Configure(EntityTypeBuilder<HallService> builder)
    {
        builder.HasKey(hs => hs.Id);

        builder.Property(hs => hs.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(hs => hs.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasData(
            new HallService
            {
                Id = InitialHallServices.HallAProjector, HallId = InitialHalls.HallAId, Name = "Проєктор", Price = 500m,
                IsDeleted = false
            },
            new HallService
            {
                Id = InitialHallServices.HallAWifi, HallId = InitialHalls.HallAId, Name = "Wi-Fi", Price = 300m,
                IsDeleted = false
            },
            new HallService
            {
                Id = InitialHallServices.HallASound, HallId = InitialHalls.HallAId, Name = "Звук", Price = 700m,
                IsDeleted = false
            },
            new HallService
            {
                Id = InitialHallServices.HallBProjector, HallId = InitialHalls.HallBId, Name = "Проєктор", Price = 500m,
                IsDeleted = false
            },
            new HallService
            {
                Id = InitialHallServices.HallBWifi, HallId = InitialHalls.HallBId, Name = "Wi-Fi", Price = 300m,
                IsDeleted = false
            },
            new HallService
            {
                Id = InitialHallServices.HallBSound, HallId = InitialHalls.HallBId, Name = "Звук", Price = 700m,
                IsDeleted = false
            },
            new HallService
            {
                Id = InitialHallServices.HallCProjector, HallId = InitialHalls.HallCId, Name = "Проєктор", Price = 500m,
                IsDeleted = false
            },
            new HallService
            {
                Id = InitialHallServices.HallCWifi, HallId = InitialHalls.HallCId, Name = "Wi-Fi", Price = 300m,
                IsDeleted = false
            },
            new HallService
            {
                Id = InitialHallServices.HallCSound, HallId = InitialHalls.HallCId, Name = "Звук", Price = 700m,
                IsDeleted = false
            }
        );
    }
}

internal static class InitialHallServices
{
    public static readonly Guid HallAProjector = Guid.Parse("a1111111-1111-1111-1111-111111111111");
    public static readonly Guid HallAWifi = Guid.Parse("a2222222-2222-2222-2222-222222222222");
    public static readonly Guid HallASound = Guid.Parse("a3333333-3333-3333-3333-333333333333");

    public static readonly Guid HallBProjector = Guid.Parse("b1111111-1111-1111-1111-111111111111");
    public static readonly Guid HallBWifi = Guid.Parse("b2222222-2222-2222-2222-222222222222");
    public static readonly Guid HallBSound = Guid.Parse("b3333333-3333-3333-3333-333333333333");

    public static readonly Guid HallCProjector = Guid.Parse("c1111111-1111-1111-1111-111111111111");
    public static readonly Guid HallCWifi = Guid.Parse("c2222222-2222-2222-2222-222222222222");
    public static readonly Guid HallCSound = Guid.Parse("c3333333-3333-3333-3333-333333333333");
}