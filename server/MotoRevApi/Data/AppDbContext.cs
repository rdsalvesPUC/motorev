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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}