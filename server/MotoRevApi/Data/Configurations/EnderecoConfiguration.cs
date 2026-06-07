using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class EnderecoConfiguration : IEntityTypeConfiguration<Endereco>
{
    public void Configure(EntityTypeBuilder<Endereco> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Cep).IsRequired().HasMaxLength(8);
        builder.Property(e => e.Logradouro).IsRequired().HasMaxLength(150);
        builder.Property(e => e.Numero).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Complemento).HasMaxLength(100);
        builder.Property(e => e.Bairro).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Cidade).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Uf).IsRequired().HasMaxLength(2);
    }
}
