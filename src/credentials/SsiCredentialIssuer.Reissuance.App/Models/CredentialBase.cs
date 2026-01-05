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

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Models;

/// <summary>
/// Interface for a credential
/// </summary>
public interface ICredential
{
    /// <summary>
    /// The type of the credential
    /// </summary>
    IEnumerable<string> Type { get; }

    /// <summary>
    /// The ID of the credential
    /// </summary>
    string Id { get; }

    /// <summary>
    /// The issuer of the credential
    /// </summary>
    string Issuer { get; }

    /// <summary>
    /// The expiration date of the credential
    /// </summary>
    string ExpirationDate { get; }

    /// <summary>
    /// The subject of the credential
    /// </summary>
    CredentialSubjectType CredentialSubject { get; }
}

/// <summary>
/// Represents the subject of a credential
/// </summary>
/// <param name="Id">The ID of the subject</param>
/// <param name="MemberOf">The membership of the subject</param>
public record CredentialSubjectType(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("memberOf")] string MemberOf
);

/// <summary>
/// Implementation of <see cref="ICredential"/>
/// </summary>
public class Credential : ICredential
{
    /// <inheritdoc />
    [JsonPropertyName("type")]
    public IEnumerable<string> Type { get; set; } = default!;

    /// <inheritdoc />
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <inheritdoc />
    [JsonPropertyName("issuer")]
    public string Issuer { get; set; } = default!;

    /// <inheritdoc />
    [JsonPropertyName("expirationDate")]
    public string ExpirationDate { get; set; } = default!;

    /// <inheritdoc />
    [JsonPropertyName("credentialSubject")]
    public CredentialSubjectType CredentialSubject { get; set; } = default!;
}
