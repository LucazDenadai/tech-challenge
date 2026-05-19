using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechChallenge.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameCpfToDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Cpf",
                table: "Clientes",
                newName: "Documento");

            migrationBuilder.RenameIndex(
                name: "IX_Clientes_Cpf",
                table: "Clientes",
                newName: "IX_Clientes_Documento");

            migrationBuilder.AlterColumn<string>(
                name: "Documento",
                table: "Clientes",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(11)",
                oldMaxLength: 11);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Documento",
                table: "Clientes",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14);

            migrationBuilder.RenameIndex(
                name: "IX_Clientes_Documento",
                table: "Clientes",
                newName: "IX_Clientes_Cpf");

            migrationBuilder.RenameColumn(
                name: "Documento",
                table: "Clientes",
                newName: "Cpf");
        }
    }
}
