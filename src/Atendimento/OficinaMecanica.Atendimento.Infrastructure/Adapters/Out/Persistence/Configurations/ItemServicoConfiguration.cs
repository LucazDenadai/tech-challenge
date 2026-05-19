using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OficinaMecanica.Atendimento.Domain.Entities;

namespace OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence.Configurations;

public class ItemServicoConfiguration : IEntityTypeConfiguration<ItemServico>
{
    public void Configure(EntityTypeBuilder<ItemServico> builder)
    {
        builder.ToTable("ItensServico");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ValorUnitario).HasColumnType("decimal(18,2)").IsRequired();
        builder.Ignore(i => i.ValorTotal);
        builder.HasOne(i => i.Servico).WithMany().HasForeignKey(i => i.ServicoId).OnDelete(DeleteBehavior.Restrict);
    }
}
