using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OficinaMecanica.Atendimento.Domain.Entities;

namespace OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence.Configurations;

public class ItemPecaConfiguration : IEntityTypeConfiguration<ItemPeca>
{
    public void Configure(EntityTypeBuilder<ItemPeca> builder)
    {
        builder.ToTable("ItensPeca");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ValorUnitario).HasColumnType("decimal(18,2)").IsRequired();
        builder.Ignore(i => i.ValorTotal);
        // PecaId referencia o microserviço Estoque — sem FK local (ADR-004)
    }
}
