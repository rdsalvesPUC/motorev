using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class LinhaConfiguration : IEntityTypeConfiguration<Linha>
{
    public void Configure(EntityTypeBuilder<Linha> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(l => l.Nome)
            .IsUnique();

        builder.Property(l => l.Descricao)
            .HasMaxLength(500);
    }
}
