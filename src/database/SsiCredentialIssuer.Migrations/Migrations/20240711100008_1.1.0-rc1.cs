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
    public partial class _110rc1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "issuer",
                table: "verified_credential_external_types",
                keyColumn: "id",
                keyValue: 4,
                column: "label",
                value: "MEMBERSHIP_CREDENTIAL");

            migrationBuilder.UpdateData(
                schema: "issuer",
                table: "verified_credential_types",
                keyColumn: "id",
                keyValue: 4,
                column: "label",
                value: "MEMBERSHIP_CERTIFICATE");

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "verified_credential_external_types",
                columns: new[] { "id", "label" },
                values: new object[] { 11, "FRAMEWORK_AGREEMENT" });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "verified_credential_types",
                columns: new[] { "id", "label" },
                values: new object[] { 11, "FRAMEWORK_AGREEMENT" });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "verified_credential_external_types",
                columns: new[] { "id", "label" },
                values: new object[] { 12, "DATA_EXCHANGE_GOVERNANCE_CREDENTIAL" });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "verified_credential_types",
                columns: new[] { "id", "label" },
                values: new object[] { 12, "DATA_EXCHANGE_GOVERNANCE_CREDENTIAL" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "issuer",
                table: "verified_credential_external_types",
                keyColumn: "id",
                keyValue: 4,
                column: "label",
                value: "VEHICLE_DISMANTLE");

            migrationBuilder.UpdateData(
                schema: "issuer",
                table: "verified_credential_types",
                keyColumn: "id",
                keyValue: 4,
                column: "label",
                value: "DISMANTLER_CERTIFICATE");

            migrationBuilder.Sql("DELETE FROM issuer.verified_credential_external_type_detail_versions WHERE verified_credential_external_type_id = 11");
            migrationBuilder.Sql("DELETE FROM issuer.verified_credential_type_assigned_external_types WHERE verified_credential_type_id = 11 OR verified_credential_external_type_id = 11");
            migrationBuilder.Sql("DELETE FROM issuer.verified_credential_type_assigned_kinds WHERE verified_credential_type_id = 11");
            migrationBuilder.Sql("DELETE FROM issuer.verified_credential_type_assigned_use_cases WHERE verified_credential_type_id = 11");

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "verified_credential_external_types",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "verified_credential_types",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.Sql("DELETE FROM issuer.verified_credential_external_type_detail_versions WHERE verified_credential_external_type_id = 12");
            migrationBuilder.Sql("DELETE FROM issuer.verified_credential_type_assigned_external_types WHERE verified_credential_type_id = 12 OR verified_credential_external_type_id = 12");
            migrationBuilder.Sql("DELETE FROM issuer.verified_credential_type_assigned_kinds WHERE verified_credential_type_id = 12");
            migrationBuilder.Sql("DELETE FROM issuer.verified_credential_type_assigned_use_cases WHERE verified_credential_type_id = 12");

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "verified_credential_external_types",
                keyColumn: "id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "verified_credential_types",
                keyColumn: "id",
                keyValue: 12);
        }
    }
}
