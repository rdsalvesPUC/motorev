using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class ServicoConfiguration : IEntityTypeConfiguration<Servico>
{
    public void Configure(EntityTypeBuilder<Servico> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Categoria)
            .IsRequired();

        builder.Property(s => s.TempoEstimado)
            .IsRequired();

        builder.Property(s => s.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.Descricao)
            .IsRequired()
            .HasMaxLength(500);


        builder.Property(s => s.Custo)
            .HasColumnType("decimal(18,2)");

        // Índice único para evitar duplicidade de código globalmente
        builder.HasIndex(s => s.Codigo)
            .IsUnique()
            .HasFilter("[Ativo] = 1");

        // Índice único para evitar duplicidade de nome dentro da mesma categoria
        builder.HasIndex(s => new { s.Categoria, s.Nome })
            .IsUnique()
            .HasFilter("[Ativo] = 1");
    }
}
