using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OficinaMecanica.Estoque.Domain.Entities;

namespace OficinaMecanica.Estoque.Infrastructure.Adapters.Out.Persistence.Configurations;

public class FalhaProcessamentoConfiguration : IEntityTypeConfiguration<FalhaProcessamento>
{
    public void Configure(EntityTypeBuilder<FalhaProcessamento> builder)
    {
        builder.ToTable("FalhasProcessamento");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.EventId).IsRequired();
        builder.Property(f => f.OrdemServicoId).IsRequired();
        builder.Property(f => f.Erro).HasMaxLength(2000).IsRequired();
        builder.Property(f => f.PayloadJson).IsRequired();
        builder.Property(f => f.OcorridoEm).IsRequired();

        builder.HasIndex(f => f.OcorridoEm);
        builder.HasIndex(f => f.OrdemServicoId);
    }
}
