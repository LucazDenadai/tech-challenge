using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OficinaMecanica.Estoque.Domain.Entities;

namespace OficinaMecanica.Estoque.Infrastructure.Adapters.Out.Persistence.Configurations;

public class MovimentacaoEstoqueConfiguration : IEntityTypeConfiguration<MovimentacaoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentacaoEstoque> builder)
    {
        builder.ToTable("MovimentacoesEstoque");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Motivo).HasMaxLength(500).IsRequired();
        builder.Property(m => m.Quantidade).IsRequired();
        builder.Property(m => m.OcorridoEm).IsRequired();

        builder.HasIndex(m => m.OsId)
               .IsUnique()
               .HasFilter("\"OsId\" IS NOT NULL");

        builder.HasOne<Peca>()
               .WithMany()
               .HasForeignKey(m => m.PecaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
