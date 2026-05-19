using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Data.Configurations;

public class ServicoConfiguration : IEntityTypeConfiguration<Servico>
{
    public void Configure(EntityTypeBuilder<Servico> builder)
    {
        builder.ToTable("Servicos");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Nome).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Descricao).HasMaxLength(500);
        builder.Property(s => s.Preco).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(s => s.Ativo).HasDefaultValue(true);
    }
}
