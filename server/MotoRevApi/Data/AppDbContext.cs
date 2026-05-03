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

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
