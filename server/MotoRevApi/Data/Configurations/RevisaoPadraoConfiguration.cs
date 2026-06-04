using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class RevisaoPadraoConfiguration : IEntityTypeConfiguration<RevisaoPadrao>
{
    public void Configure(EntityTypeBuilder<RevisaoPadrao> builder)
    {
        builder.HasIndex(rp => rp.ModeloMotoId);
        builder.HasIndex(rp => rp.ConcessionariaId);
    }
}
