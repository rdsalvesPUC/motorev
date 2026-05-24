using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class MotoConfiguration : IEntityTypeConfiguration<Moto>
{
    public void Configure(EntityTypeBuilder<Moto> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Placa)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.Chassi)
            .IsRequired()
            .HasMaxLength(17);

        builder.Property(m => m.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.Foto)
            .IsRequired(false);

        // Relacionamentos
        builder.HasOne(m => m.Cliente)
            .WithMany()
            .HasForeignKey(m => m.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.ModeloMoto)
            .WithMany()
            .HasForeignKey(m => m.ModeloMotoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Concessionaria)
            .WithMany()
            .HasForeignKey(m => m.ConcessionariaId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Índices únicos condicionais para chassi e placa ativos
        builder.HasIndex(m => m.Placa)
            .IsUnique()
            .HasFilter("[Ativo] = 1");

        builder.HasIndex(m => m.Chassi)
            .IsUnique()
            .HasFilter("[Ativo] = 1");
    }
}