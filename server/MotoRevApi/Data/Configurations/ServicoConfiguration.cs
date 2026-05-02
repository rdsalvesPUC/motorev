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

        // Índice único para evitar duplicidade de serviço com mesmo nome e categoria globalmente
        builder.HasIndex(s => new { s.Nome, s.Categoria })
            .IsUnique();
    }
}
