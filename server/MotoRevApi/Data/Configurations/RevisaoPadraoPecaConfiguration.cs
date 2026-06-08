using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class RevisaoPadraoPecaConfiguration : IEntityTypeConfiguration<RevisaoPadraoPeca>
{
    public void Configure(EntityTypeBuilder<RevisaoPadraoPeca> builder)
    {
        builder.HasKey(rpp => new { rpp.RevisaoPadraoId, rpp.PecaId });

        builder.Property(rpp => rpp.Quantidade)
            .IsRequired();

        builder.HasOne(rpp => rpp.RevisaoPadrao)
            .WithMany(rp => rp.Pecas)
            .HasForeignKey(rpp => rpp.RevisaoPadraoId);

        builder.HasOne(rpp => rpp.Peca)
            .WithMany(p => p.RevisoesPadrao)
            .HasForeignKey(rpp => rpp.PecaId);
    }
}
