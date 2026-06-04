using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class PecaConfiguration : IEntityTypeConfiguration<Peca>
{
    public void Configure(EntityTypeBuilder<Peca> builder)
    {
        builder
            .HasKey(p => p.Id);
        builder
            .Property(p => p.Codigo)
            .HasMaxLength(20)
            .IsRequired();
        builder
            .Property(p => p.Nome)
            .HasMaxLength(150)
            .IsRequired();
        builder
            .Property(p => p.Categoria)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder
            .Property(p => p.Preco)
            .HasPrecision(18, 2)
            .IsRequired();
        builder
            .Property(p => p.Estoque)
            .IsRequired();
        builder
            .Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder
            .HasIndex(p => p.Codigo)
            .IsUnique();
        builder
            .HasIndex(p => p.Nome);
        builder
            .HasIndex(p => p.Status);
        builder
            .ToTable("Pecas");
    }
}
