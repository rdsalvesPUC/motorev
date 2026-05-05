using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MotoRevApi.Model;

namespace MotoRevApi.Data;

public class AppDbContext : IdentityDbContext<Usuario>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Moto> Motos { get; set; }
    public DbSet<ModeloMoto> ModelosMotos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Concessionaria> Concessionarias { get; set; }
    public DbSet<Endereco> Enderecos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Define que a coluna Cnpj deve ser única no banco de dados
        modelBuilder.Entity<Concessionaria>()
            .HasIndex(c => c.Cnpj)
            .IsUnique();

        // Configuração explícita do relacionamento 1:1 entre Usuario e Concessionaria
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Concessionaria)
            .WithOne(c => c.Usuario)
            .HasForeignKey<Concessionaria>(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade); // Se o usuário for deletado, apaga a concessionária junto

        // Configuração explícita do relacionamento 1:1 entre Usuario e Cliente (já deixando pronto)
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Cliente)
            .WithOne(c => c.Usuario)
            .HasForeignKey<Cliente>(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configuração do relacionamento 1:N entre Concessionaria e Enderecos
        modelBuilder.Entity<Concessionaria>()
            .HasMany(c => c.Enderecos)
            .WithOne(e => e.Concessionaria)
            .HasForeignKey(e => e.ConcessionariaId)
            .OnDelete(DeleteBehavior.Cascade); // Apagar concessionária apaga os endereços

        // Global Query Filters para Soft Delete (Ignorar registros inativos em qualquer busca)
        modelBuilder.Entity<Concessionaria>().HasQueryFilter(c => c.Ativo);
        modelBuilder.Entity<Endereco>().HasQueryFilter(e => e.Ativo);
        modelBuilder.Entity<Usuario>().HasQueryFilter(u => u.Ativo);
        // Aplica o global filter para as outras entidades que tem a propriedade Ativo
        modelBuilder.Entity<ModeloMoto>().HasQueryFilter(m => m.Ativo);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
