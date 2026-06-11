using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OficinaMecanica.Atendimento.Domain.Entities;

namespace OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence.Configurations;

public class OrdemServicoConfiguration : IEntityTypeConfiguration<OrdemServico>
{
    public void Configure(EntityTypeBuilder<OrdemServico> builder)
    {
        builder.ToTable("OrdensServico");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Numero).HasMaxLength(20).IsRequired();
        builder.HasIndex(o => o.Numero).IsUnique();
        builder.Property(o => o.Status).IsRequired();
        builder.Property(o => o.Observacoes).HasMaxLength(500);
        builder.HasOne(o => o.Cliente).WithMany().HasForeignKey(o => o.ClienteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(o => o.Veiculo).WithMany().HasForeignKey(o => o.VeiculoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(o => o.ItensServico).WithOne(i => i.OrdemServico).HasForeignKey(i => i.OrdemServicoId);
        builder.HasMany(o => o.ItensPeca).WithOne(i => i.OrdemServico).HasForeignKey(i => i.OrdemServicoId);

        builder.Navigation(o => o.ItensServico).HasField("_itensServico").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(o => o.ItensPeca).HasField("_itensPeca").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(o => o.Historico).WithOne().HasForeignKey(h => h.OrdemServicoId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Historico).HasField("_historico").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
