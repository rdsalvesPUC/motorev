using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class MotoConfiguration : IEntityTypeConfiguration<Moto>
{
    public void Configure(EntityTypeBuilder<Moto> builder)
    {
        builder
            .HasKey(m => m.Id);
        builder
            .Property(m => m.Modelo)
            .HasMaxLength(50)
            .IsRequired();
        builder
            .Property(m => m.Cor)
            .HasMaxLength(20)
            .IsRequired(false);
        builder
            .Property(m => m.Ano)
            .HasMaxLength(4)
            .IsRequired();
    }
}