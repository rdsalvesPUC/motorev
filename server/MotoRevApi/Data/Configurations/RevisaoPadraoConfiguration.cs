using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class RevisaoPadraoConfiguration : IEntityTypeConfiguration<RevisaoPadrao>
{
    public void Configure(EntityTypeBuilder<RevisaoPadrao> builder)
    {
        builder.HasIndex(rp => rp.LinhaId);
        builder.HasIndex(rp => new { rp.LinhaId, rp.Ordem }).IsUnique();

        builder.HasOne(rp => rp.Linha)
            .WithMany()
            .HasForeignKey(rp => rp.LinhaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
