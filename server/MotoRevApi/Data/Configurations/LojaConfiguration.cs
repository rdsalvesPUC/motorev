using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class LojaConfiguration : IEntityTypeConfiguration<Loja>
{
    public void Configure(EntityTypeBuilder<Loja> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Nome).IsRequired().HasMaxLength(150);
        builder.Property(l => l.Tipo).IsRequired().HasMaxLength(20).HasDefaultValue("Filial");
        builder.Property(l => l.Cnpj).IsRequired().HasMaxLength(18);
        builder.HasIndex(l => l.Cnpj).IsUnique();
        builder.Property(l => l.Telefone).IsRequired().HasMaxLength(20);
        builder.HasIndex(l => l.Telefone).IsUnique();
        builder.Property(l => l.Cep).IsRequired().HasMaxLength(9);
        builder.Property(l => l.Logradouro).IsRequired().HasMaxLength(150);
        builder.Property(l => l.Numero).IsRequired().HasMaxLength(20);
        builder.Property(l => l.Bairro).IsRequired().HasMaxLength(100);
        builder.Property(l => l.Cidade).IsRequired().HasMaxLength(100);
        builder.Property(l => l.Uf).IsRequired().HasMaxLength(2);
        builder.Property(l => l.Foto);
        builder.Property(l => l.Ativo).IsRequired().HasDefaultValue(true);
        builder.HasIndex(l => l.ConcessionariaId);
        builder.HasIndex(l => new { l.ConcessionariaId, l.Tipo })
            .IsUnique()
            .HasFilter("[Tipo] = 'Matriz'");
    }
}
