using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Data.Configurations;

public class ItemPecaConfiguration : IEntityTypeConfiguration<ItemPeca>
{
    public void Configure(EntityTypeBuilder<ItemPeca> builder)
    {
        builder.ToTable("ItensPeca");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ValorUnitario).HasColumnType("decimal(18,2)").IsRequired();
        builder.Ignore(i => i.ValorTotal);
        builder.HasOne(i => i.Peca).WithMany().HasForeignKey(i => i.PecaId).OnDelete(DeleteBehavior.Restrict);
    }
}
