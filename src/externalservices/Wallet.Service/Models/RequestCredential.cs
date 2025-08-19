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

public record RequestCredential(
    [property: JsonPropertyName("requestedCredentials")] IEnumerable<RequestedCredentials> Data,
    [property: JsonPropertyName("issuerDid")] string IssuerDid,
    [property: JsonPropertyName("holderDid")] string HolderDid,
    [property: JsonPropertyName("expirationDate")] string ExpirationDate
);

public record RequestedCredentials(
    [property: JsonPropertyName("credentialType")] string CredentialType,
    [property: JsonPropertyName("format")] string Format
);

public record RequestCredentialResponse(
    [property: JsonPropertyName("holderRequestId")] Guid Id
);
