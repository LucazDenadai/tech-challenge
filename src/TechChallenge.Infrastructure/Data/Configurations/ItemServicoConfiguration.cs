using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Data.Configurations;

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
