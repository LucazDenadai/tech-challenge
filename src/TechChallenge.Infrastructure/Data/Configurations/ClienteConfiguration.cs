using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Infrastructure.Data.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nome).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Cpf).HasMaxLength(11).IsRequired();
        builder.HasIndex(c => c.Cpf).IsUnique();
        builder.Property(c => c.Email).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Telefone).HasMaxLength(20);
        builder.Property(c => c.Endereco).HasMaxLength(200);
        builder.Property(c => c.Ativo).HasDefaultValue(true);
        builder.HasMany(c => c.Veiculos).WithOne(v => v.Cliente).HasForeignKey(v => v.ClienteId);
    }
}
