using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class RevisaoPadraoServicoConfiguration : IEntityTypeConfiguration<RevisaoPadraoServico>
{
    public void Configure(EntityTypeBuilder<RevisaoPadraoServico> builder)
    {
        builder.HasKey(rps => new { rps.RevisaoPadraoId, rps.ServicoId });

        builder.HasOne(rps => rps.RevisaoPadrao)
            .WithMany(rp => rp.Servicos)
            .HasForeignKey(rps => rps.RevisaoPadraoId);

        builder.HasOne(rps => rps.Servico)
            .WithMany(s => s.RevisoesPadrao)
            .HasForeignKey(rps => rps.ServicoId);
    }
}
