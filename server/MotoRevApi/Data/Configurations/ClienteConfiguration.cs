using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nome).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Cpf).HasMaxLength(11);
        builder.Property(c => c.Cep).HasMaxLength(8);
        builder.Property(c => c.Logradouro).HasMaxLength(150);
        builder.Property(c => c.Numero).HasMaxLength(20);
        builder.Property(c => c.Complemento).HasMaxLength(100);
        builder.Property(c => c.Bairro).HasMaxLength(100);
        builder.Property(c => c.Cidade).HasMaxLength(100);
        builder.Property(c => c.Uf).HasMaxLength(2);
        builder.HasIndex(c => c.Cpf).IsUnique().HasFilter("[Cpf] IS NOT NULL");

        // Relacionamento 1:1 com Usuario
        builder.HasOne(c => c.Usuario)
            .WithOne()
            .HasForeignKey<Cliente>(c => c.UsuarioId)
            .IsRequired();
    }
}
