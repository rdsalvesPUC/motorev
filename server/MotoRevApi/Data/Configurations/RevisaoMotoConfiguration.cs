using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class RevisaoMotoConfiguration : IEntityTypeConfiguration<RevisaoMoto>
{
    public void Configure(EntityTypeBuilder<RevisaoMoto> builder)
    {
        builder.HasKey(rm => rm.Id);

        builder.Property(rm => rm.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rm => rm.Status)
            .IsRequired()
            .HasMaxLength(40);

        builder.HasIndex(rm => rm.MotoId);
        builder.HasIndex(rm => new { rm.MotoId, rm.Ordem }).IsUnique();

        builder.HasOne(rm => rm.Moto)
            .WithMany(m => m.RevisoesPlanejadas)
            .HasForeignKey(rm => rm.MotoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rm => rm.RevisaoPadrao)
            .WithMany()
            .HasForeignKey(rm => rm.RevisaoPadraoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
