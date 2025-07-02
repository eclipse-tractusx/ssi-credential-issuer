/********************************************************************************
 * Copyright (c) 2025 Cofinity-X GmbH
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Models;

public record GetCredentialRequestReceivedResponse(
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("data")] IEnumerable<CredentialRequestReceived> Data
);

public record CredentialRequestReceived(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("expirationDate")] string ExpirationDate,
    [property: JsonPropertyName("requestedCredentials")] IEnumerable<RequestedCredentialsType> RequestedCredentials,
    [property: JsonPropertyName("matchingCredentials")] IEnumerable<Credential> MatchingCredentials,
    [property: JsonPropertyName("issuerDid")] string IssuerDid,
    [property: JsonPropertyName("holderDid")] string HolderDid,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("deliveryStatus")] string DeliveryStatus,
    [property: JsonPropertyName("approvedCredentials")] string[]? ApprovedCredentials
);

public record RequestedCredentialsType(
    [property: JsonPropertyName("format")] string Format,
    [property: JsonPropertyName("credentialType")] string CredentialType
);

public record RequestedCredentialAutoApproveResponse(
    [property: JsonPropertyName("issuerPid")] string IssuerPid,
    [property: JsonPropertyName("holderPid")] string HolderPid,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("reason")] string? Reason
);