using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class ModeloMotoConfiguration : IEntityTypeConfiguration<ModeloMoto>
{
    public void Configure(EntityTypeBuilder<ModeloMoto> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.NomeModelo)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(m => m.NomeModelo)
            .IsUnique();

        builder.Property(m => m.Marca)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(m => m.Marca);

        builder.Property(m => m.Categoria)
            .HasMaxLength(50);
    }
}