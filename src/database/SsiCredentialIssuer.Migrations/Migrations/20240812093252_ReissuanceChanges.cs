/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

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
