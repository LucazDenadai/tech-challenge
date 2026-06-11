using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OficinaMecanica.Estoque.Infrastructure.Adapters.Out.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFalhaProcessamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FalhasProcessamento",
                schema: "estoque",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Erro = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    OcorridoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FalhasProcessamento", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FalhasProcessamento_OcorridoEm",
                schema: "estoque",
                table: "FalhasProcessamento",
                column: "OcorridoEm");

            migrationBuilder.CreateIndex(
                name: "IX_FalhasProcessamento_OrdemServicoId",
                schema: "estoque",
                table: "FalhasProcessamento",
                column: "OrdemServicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FalhasProcessamento",
                schema: "estoque");
        }
    }
}
