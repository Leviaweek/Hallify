using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HallifyDatabase.Models;

[Serializable]
[Table("Halls",  Schema = HallDbContext.PublicSchema)]
public class Hall
{
    [Key] public Guid Id { get; set; }
    public required string Name { get; set; }
    
    public int Capacity { get; set; }
    
    public decimal HourlyRate { get; set; }
}

file sealed class HallConfigure: IEntityTypeConfiguration<Hall>
{
    public void Configure(EntityTypeBuilder<Hall> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.Capacity)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}