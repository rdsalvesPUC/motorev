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

        // Relacionamento 1:1 com Usuario
        builder.HasOne(c => c.Usuario)
            .WithOne()
            .HasForeignKey<Cliente>(c => c.UsuarioId)
            .IsRequired();
    }
}
