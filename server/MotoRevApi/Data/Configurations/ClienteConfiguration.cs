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
        builder.HasIndex(c => c.Cpf).IsUnique().HasFilter("[Cpf] IS NOT NULL");
        builder.HasIndex(c => c.EnderecoId).IsUnique().HasFilter("[EnderecoId] IS NOT NULL");

        // Relacionamento 1:1 com Usuario
        builder.HasOne(c => c.Usuario)
            .WithOne()
            .HasForeignKey<Cliente>(c => c.UsuarioId)
            .IsRequired();

        builder.HasOne(c => c.Endereco)
            .WithOne()
            .HasForeignKey<Cliente>(c => c.EnderecoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
