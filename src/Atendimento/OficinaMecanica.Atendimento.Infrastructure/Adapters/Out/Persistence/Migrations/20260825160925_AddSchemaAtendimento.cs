using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OficinaMecanica.Atendimento.Infrastructure.Adapters.Out.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemaAtendimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "atendimento");

            migrationBuilder.RenameTable(
                name: "Veiculos",
                newName: "Veiculos",
                newSchema: "atendimento");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "Usuarios",
                newSchema: "atendimento");

            migrationBuilder.RenameTable(
                name: "Servicos",
                newName: "Servicos",
                newSchema: "atendimento");

            migrationBuilder.RenameTable(
                name: "OrdensServico",
                newName: "OrdensServico",
                newSchema: "atendimento");

            migrationBuilder.RenameTable(
                name: "ItensServico",
                newName: "ItensServico",
                newSchema: "atendimento");

            migrationBuilder.RenameTable(
                name: "ItensPeca",
                newName: "ItensPeca",
                newSchema: "atendimento");

            migrationBuilder.RenameTable(
                name: "HistoricoStatusOS",
                newName: "HistoricoStatusOS",
                newSchema: "atendimento");

            migrationBuilder.RenameTable(
                name: "Clientes",
                newName: "Clientes",
                newSchema: "atendimento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Veiculos",
                schema: "atendimento",
                newName: "Veiculos");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                schema: "atendimento",
                newName: "Usuarios");

            migrationBuilder.RenameTable(
                name: "Servicos",
                schema: "atendimento",
                newName: "Servicos");

            migrationBuilder.RenameTable(
                name: "OrdensServico",
                schema: "atendimento",
                newName: "OrdensServico");

            migrationBuilder.RenameTable(
                name: "ItensServico",
                schema: "atendimento",
                newName: "ItensServico");

            migrationBuilder.RenameTable(
                name: "ItensPeca",
                schema: "atendimento",
                newName: "ItensPeca");

            migrationBuilder.RenameTable(
                name: "HistoricoStatusOS",
                schema: "atendimento",
                newName: "HistoricoStatusOS");

            migrationBuilder.RenameTable(
                name: "Clientes",
                schema: "atendimento",
                newName: "Clientes");
        }
    }
}
