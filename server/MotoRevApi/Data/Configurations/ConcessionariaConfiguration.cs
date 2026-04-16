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
        builder.Property(c => c.Cnpj).IsRequired().HasMaxLength(14);
        builder.HasIndex(c => c.Cnpj).IsUnique();
        builder.Property(c => c.Tel).IsRequired().HasMaxLength(11);

        // Relacionamento 1:1 com Usuario
        builder.HasOne(c => c.Usuario)
            .WithOne()
            .HasForeignKey<Concessionaria>(c => c.UsuarioId)
            .IsRequired();
            
        // Relacionamento 1:N com Endereco
        builder.HasMany(c => c.Enderecos)
            .WithOne()
            .IsRequired();
    }
}
