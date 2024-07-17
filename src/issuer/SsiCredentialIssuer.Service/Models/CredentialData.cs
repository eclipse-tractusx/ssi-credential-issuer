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

using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Models;

public record FrameworkCredential(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("@context")] IEnumerable<string> Context,
    [property: JsonPropertyName("type")] IEnumerable<string> Type,
    [property: JsonPropertyName("issuanceDate")] DateTimeOffset IssuanceDate,
    [property: JsonPropertyName("expirationDate")] DateTimeOffset ExpirationDate,
    [property: JsonPropertyName("issuer")] string Issuer,
    [property: JsonPropertyName("credentialSubject")] FrameworkCredentialSubject CredentialSubject,
    [property: JsonPropertyName("credentialStatus")] CredentialStatus CredentialStatus);

public record FrameworkCredentialSubject(
    [property: JsonPropertyName("id")] string Did,
    [property: JsonPropertyName("holderIdentifier")] string HolderIdentifier,
    [property: JsonPropertyName("group")] string Group,
    [property: JsonPropertyName("useCase")] string UseCase,
    [property: JsonPropertyName("contractTemplate")] string ContractTemplate,
    [property: JsonPropertyName("contractVersion")] string ContractVersion
);

public record MembershipCredential(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("@context")] IEnumerable<string> Context,
    [property: JsonPropertyName("type")] IEnumerable<string> Type,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("issuanceDate")] DateTimeOffset IssuanceDate,
    [property: JsonPropertyName("expirationDate")] DateTimeOffset ExpirationDate,
    [property: JsonPropertyName("issuer")] string Issuer,
    [property: JsonPropertyName("credentialSubject")] MembershipCredentialSubject CredentialSubject,
    [property: JsonPropertyName("credentialStatus")] CredentialStatus CredentialStatus);

public record MembershipCredentialSubject(
    [property: JsonPropertyName("id")] string Did,
    [property: JsonPropertyName("holderIdentifier")] string HolderIdentifier,
    [property: JsonPropertyName("memberOf")] string MemberOf
);

public record BpnCredential(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("@context")] IEnumerable<string> Context,
    [property: JsonPropertyName("type")] IEnumerable<string> Type,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("issuanceDate")] DateTimeOffset IssuanceDate,
    [property: JsonPropertyName("expirationDate")] DateTimeOffset ExpirationDate,
    [property: JsonPropertyName("issuer")] string Issuer,
    [property: JsonPropertyName("credentialSubject")] BpnCredentialSubject CredentialSubject,
    [property: JsonPropertyName("credentialStatus")] CredentialStatus CredentialStatus);

public record CredentialStatus(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type
);

public record BpnCredentialSubject(
    [property: JsonPropertyName("id")] string Did,
    [property: JsonPropertyName("holderIdentifier")] string HolderIdentifier,
    [property: JsonPropertyName("bpn")] string Bpn
);
