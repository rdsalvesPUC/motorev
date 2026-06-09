using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class ConcessionariaConfiguration : IEntityTypeConfiguration<Concessionaria>
{
    public void Configure(EntityTypeBuilder<Concessionaria> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nome).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Cnpj).IsRequired().HasMaxLength(18);
        builder.HasIndex(c => c.Cnpj).IsUnique();
        builder.Property(c => c.Telefone).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Tipo).IsRequired().HasMaxLength(20).HasDefaultValue("Matriz");

        builder.HasMany(c => c.Lojas)
            .WithOne(l => l.Concessionaria)
            .HasForeignKey(l => l.ConcessionariaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
