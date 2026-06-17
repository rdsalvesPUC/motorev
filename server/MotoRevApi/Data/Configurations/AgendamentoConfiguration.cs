using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MotoRevApi.Enums;
using MotoRevApi.Model;

namespace MotoRevApi.Data.Configurations;

public class AgendamentoConfiguration : IEntityTypeConfiguration<Agendamento>
{
    public void Configure(EntityTypeBuilder<Agendamento> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(a => a.DataAgendada)
            .IsRequired();

        builder.Property(a => a.MensagemRecusa)
            .HasMaxLength(500);

        builder.Property(a => a.CriadoEm)
            .IsRequired();

        builder.Property(a => a.AtualizadoEm)
            .IsRequired();

        builder.HasIndex(a => a.RevisaoMotoId);
        builder.HasIndex(a => a.LojaId);
        builder.HasIndex(a => new { a.LojaId, a.DataAgendada, a.Status });
        builder.HasIndex(a => new { a.RevisaoMotoId, a.Status })
            .HasFilter("[Status] IN ('AguardandoConfirmacao', 'Agendada', 'EmExecucao')");

        builder.HasOne(a => a.RevisaoMoto)
            .WithMany()
            .HasForeignKey(a => a.RevisaoMotoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Loja)
            .WithMany()
            .HasForeignKey(a => a.LojaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
