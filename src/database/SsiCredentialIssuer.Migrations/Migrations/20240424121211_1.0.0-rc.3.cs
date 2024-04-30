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

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _100rc3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "issuer",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 100, "REVOKE_CREDENTIAL" },
                    { 101, "TRIGGER_NOTIFICATION" },
                    { 102, "TRIGGER_MAIL" }
                });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 2, "DECLINE_CREDENTIAL" });

            migrationBuilder.UpdateData(
                schema: "issuer",
                table: "verified_credential_external_types",
                keyColumn: "id",
                keyValue: 5,
                column: "label",
                value: "CIRCULAR_ECONOMY");

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "verified_credential_external_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 8, "DEMAND_AND_CAPACITY_MANAGEMENT" },
                    { 9, "DEMAND_AND_CAPACITY_MANAGEMENT_PURIS" },
                    { 10, "BUSINESS_PARTNER_DATA_MANAGEMENT" }
                });

            migrationBuilder.UpdateData(
                schema: "issuer",
                table: "verified_credential_types",
                keyColumn: "id",
                keyValue: 5,
                column: "label",
                value: "CIRCULAR_ECONOMY");

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "verified_credential_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 8, "DEMAND_AND_CAPACITY_MANAGEMENT" },
                    { 9, "DEMAND_AND_CAPACITY_MANAGEMENT_PURIS" },
                    { 10, "BUSINESS_PARTNER_DATA_MANAGEMENT" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "process_types",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "verified_credential_external_types",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "verified_credential_external_types",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "verified_credential_external_types",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "verified_credential_types",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "verified_credential_types",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "verified_credential_types",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.UpdateData(
                schema: "issuer",
                table: "verified_credential_external_types",
                keyColumn: "id",
                keyValue: 5,
                column: "label",
                value: "SUSTAINABILITY_CREDENTIAL");

            migrationBuilder.UpdateData(
                schema: "issuer",
                table: "verified_credential_types",
                keyColumn: "id",
                keyValue: 5,
                column: "label",
                value: "SUSTAINABILITY_FRAMEWORK");
        }
    }
}
