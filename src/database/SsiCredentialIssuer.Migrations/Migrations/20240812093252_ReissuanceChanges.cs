using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ReissuanceChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reissuances",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reissued_credential_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reissuances", x => x.id);
                    table.ForeignKey(
                        name: "fk_reissuances_company_ssi_details_id",
                        column: x => x.id,
                        principalSchema: "issuer",
                        principalTable: "company_ssi_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[] { 6, "REVOKE_REISSUED_CREDENTIAL" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reissuances",
                schema: "issuer");

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 6);
        }
    }
}
