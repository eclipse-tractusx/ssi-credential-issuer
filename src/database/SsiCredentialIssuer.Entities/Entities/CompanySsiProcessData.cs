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

using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;

public class CompanySsiProcessData
{
    private CompanySsiProcessData()
    {
        Schema = null!;
    }

    public CompanySsiProcessData(Guid companySsiDetailId, JsonDocument schema, VerifiedCredentialTypeKindId credentialTypeKindId)
        : this()
    {
        CompanySsiDetailId = companySsiDetailId;
        Schema = schema;
        CredentialTypeKindId = credentialTypeKindId;
    }

    public Guid CompanySsiDetailId { get; set; }
    public JsonDocument Schema { get; set; }
    public VerifiedCredentialTypeKindId CredentialTypeKindId { get; set; }
    public string? ClientId { get; set; }
    public byte[]? ClientSecret { get; set; }
    public byte[]? InitializationVector { get; set; }
    public int? EncryptionMode { get; set; }
    public string? HolderWalletUrl { get; set; }
    public string? CallbackUrl { get; set; }
    public virtual CompanySsiDetail? CompanySsiDetail { get; private set; }
    public virtual VerifiedCredentialTypeKind? CredentialTypeKind { get; private set; }
}
