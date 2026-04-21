using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class ConcessionariaConfiguration : IEntityTypeConfiguration<Concessionaria>
{
    public void Configure(EntityTypeBuilder<Concessionaria> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nome).IsRequired().HasMaxLength(150);

        // Relacionamento 1:1 com Usuario
        builder.HasOne(c => c.Usuario)
            .WithOne()
            .HasForeignKey<Concessionaria>(c => c.UsuarioId)
            .IsRequired();
    }
}
