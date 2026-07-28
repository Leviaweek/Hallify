using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HallifyDatabase.Models;

[Serializable]
[Table("HallServices",  Schema = HallDbContext.PublicSchema)]
public class HallService
{
    [Key] public Guid Id { get; set; }

    public Guid HallId { get; set; }
    public Hall Hall { get; set; } = null!;

    [MaxLength(50)]public required string Name { get; set; }
    
    public decimal Price { get; set; }
    public bool IsDeleted { get; set; }
}

file sealed class HallServiceConfigure: IEntityTypeConfiguration<HallService>
{
    public void Configure(EntityTypeBuilder<HallService> builder)
    {
        builder.HasKey(hs => hs.Id);
        
        builder.Property(hs => hs.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(hs  => hs.Price)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.HasOne(hs => hs.Hall)
            .WithMany(h => h.HallServices)
            .HasForeignKey(hs => hs.HallId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}