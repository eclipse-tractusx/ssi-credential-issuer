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

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Handlers;

using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;

public class IssuerCredentialRequest(
    Guid id,
    string bpnl,
    VerifiedCredentialTypeKindId kindId,
    VerifiedCredentialTypeId typeId,
    DateTimeOffset expiryDate,
    string identiyId,
    string schema,
    string? holderWalletUrl,
    Guid? detailVersionId,
    string? callbackUrl)
{
    public Guid Id { get; } = id;
    public string Bpnl { get; } = bpnl;
    public VerifiedCredentialTypeKindId KindId { get; } = kindId;
    public VerifiedCredentialTypeId TypeId { get; } = typeId;
    public DateTimeOffset ExpiryDate { get; } = expiryDate;
    public string IdentiyId { get; } = identiyId;
    public string Schema { get; } = schema;
    public string? HolderWalletUrl { get; } = holderWalletUrl;
    public Guid? DetailVersionId { get; } = detailVersionId;
    public string? CallbackUrl { get; } = callbackUrl;
}
