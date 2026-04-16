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
        builder.Property(c => c.Cpf).IsRequired().HasMaxLength(11);
        builder.HasIndex(c => c.Cpf).IsUnique();
        builder.Property(c => c.Email).IsRequired().HasMaxLength(150);
        builder.HasIndex(c => c.Email).IsUnique();
        builder.Property(c => c.Tel).HasMaxLength(11);
        builder.Property(c => c.Cel).IsRequired().HasMaxLength(11);

        // Relacionamento 1:1 com Usuario
        builder.HasOne(c => c.Usuario)
            .WithOne()
            .HasForeignKey<Cliente>(c => c.UsuarioId)
            .IsRequired();
            
        // Relacionamento 1:1 com Endereco (opcional)
        builder.HasOne(c => c.Endereco)
            .WithMany()
            .IsRequired(false);
    }
}
