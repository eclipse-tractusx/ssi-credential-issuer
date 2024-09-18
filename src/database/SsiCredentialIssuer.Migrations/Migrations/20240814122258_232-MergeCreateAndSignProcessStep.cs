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
    public partial class _232MergeCreateAndSignProcessStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.UpdateData(
                schema: "issuer",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 1,
                column: "label",
                value: "CREATE_SIGNED_CREDENTIAL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "issuer",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 1,
                column: "label",
                value: "CREATE_CREDENTIAL");

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[] { 2, "SIGN_CREDENTIAL" });
        }
    }
}
