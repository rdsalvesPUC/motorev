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
            .IsUnique()
            .HasFilter("[Ativo] = 1");

        builder.Property(m => m.Marca)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(m => m.Marca);

        builder.HasOne(m => m.Linha)
            .WithMany()
            .HasForeignKey(m => m.LinhaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
