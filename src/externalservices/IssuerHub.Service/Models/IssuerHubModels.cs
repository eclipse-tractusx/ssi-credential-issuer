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

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.IssuerHub.Service.Models;

// --- Participant Context ---

public record CreateParticipantContextRequest(
    [property: JsonPropertyName("participantId")] string ParticipantId,
    [property: JsonPropertyName("did")] string Did,
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("key")] KeyDescriptor Key,
    [property: JsonPropertyName("roles")] IEnumerable<string> Roles);

public record KeyDescriptor(
    [property: JsonPropertyName("keyId")] string KeyId,
    [property: JsonPropertyName("privateKeyAlias")] string PrivateKeyAlias,
    [property: JsonPropertyName("keyGeneratorParams")] KeyGeneratorParams KeyGeneratorParams);

public record KeyGeneratorParams(
    [property: JsonPropertyName("algorithm")] string Algorithm,
    [property: JsonPropertyName("curve")] string Curve);

public record ParticipantContextResponse(
    [property: JsonPropertyName("participantId")] string ParticipantId,
    [property: JsonPropertyName("did")] string Did,
    [property: JsonPropertyName("apiTokenAlias")] string? ApiTokenAlias);

// --- Attestation Definition ---

public record CreateAttestationDefinitionRequest(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("attestationType")] string AttestationType,
    [property: JsonPropertyName("configuration")] AttestationConfiguration Configuration);

public record AttestationConfiguration(
    [property: JsonPropertyName("credentialType")] string CredentialType);

// --- Credential Definition ---

public record CreateCredentialDefinitionRequest(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("credentialType")] string CredentialType,
    [property: JsonPropertyName("format")] string Format,
    [property: JsonPropertyName("attestations")] IEnumerable<string> Attestations,
    [property: JsonPropertyName("jsonSchema")] string JsonSchema,
    [property: JsonPropertyName("jsonSchemaUrl")] string? JsonSchemaUrl,
    [property: JsonPropertyName("mappings")] IEnumerable<CredentialMapping> Mappings,
    [property: JsonPropertyName("validity")] long Validity);

public record CredentialMapping(
    [property: JsonPropertyName("input")] string Input,
    [property: JsonPropertyName("output")] string Output,
    [property: JsonPropertyName("required")] bool Required);

// --- Holder ---

public record RegisterHolderRequest(
    [property: JsonPropertyName("holderId")] string HolderId,
    [property: JsonPropertyName("did")] string Did,
    [property: JsonPropertyName("name")] string Name);

// --- Credential Offer ---

public record TriggerCredentialOfferRequest(
    [property: JsonPropertyName("holderId")] string HolderId,
    [property: JsonPropertyName("credentials")] IEnumerable<string> Credentials);

// --- Credential Status ---

public record CredentialStatusResponse(
    [property: JsonPropertyName("status")] string Status);

// --- Credential Request (DCP) ---

public record CredentialRequestMessage(
    [property: JsonPropertyName("@context")] IEnumerable<string> Context,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("credentials")] IEnumerable<CredentialRequestEntry> Credentials,
    [property: JsonPropertyName("issuerPid")] string IssuerPid,
    [property: JsonPropertyName("holderPid")] string HolderPid,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("requestId")] string? RequestId);

public record CredentialRequestEntry(
    [property: JsonPropertyName("credentialType")] string CredentialType,
    [property: JsonPropertyName("payload")] string Payload,
    [property: JsonPropertyName("format")] string Format);
