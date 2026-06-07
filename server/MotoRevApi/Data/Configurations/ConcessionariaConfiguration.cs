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
        builder.Property(c => c.Cnpj).IsRequired().HasMaxLength(18);
        builder.HasIndex(c => c.Cnpj).IsUnique();
        builder.Property(c => c.Telefone).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Tipo).IsRequired().HasMaxLength(20).HasDefaultValue("Matriz");
        builder.Property(c => c.Cep).IsRequired().HasMaxLength(9);
        builder.Property(c => c.Logradouro).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Numero).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Bairro).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Cidade).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Uf).IsRequired().HasMaxLength(2);

        // Relacionamento 1:1 com Usuario
        builder.HasOne(c => c.Usuario)
            .WithOne()
            .HasForeignKey<Concessionaria>(c => c.UsuarioId)
            .IsRequired();

        builder.HasMany(c => c.Lojas)
            .WithOne(l => l.Concessionaria)
            .HasForeignKey(l => l.ConcessionariaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
