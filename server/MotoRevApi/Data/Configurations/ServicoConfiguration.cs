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

        // Índice único para evitar duplicidade e otimizar buscas por Categoria e Nome
        builder.HasIndex(s => new { s.Categoria, s.Nome })
            .IsUnique();
    }
}
