/********************************************************************************
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

using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using System.Text.Json;
using EncryptionInformation = Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Models.EncryptionInformation;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.BusinessLogic;

public interface IWalletBusinessLogic
{
    Task CreateSignedCredential(Guid companySsiDetailId, JsonDocument schema, CancellationToken cancellationToken);

    Task CreateCredentialForHolder(Guid companySsiDetailId, string holderWalletUrl, string clientId, EncryptionInformation encryptionInformation, string credential, CancellationToken cancellationToken);

    Task GetCredential(Guid credentialId, Guid externalCredentialId, VerifiedCredentialTypeKindId kindId, CancellationToken cancellationToken);

    Task RequestCredentialForHolder(Guid companySsiDetailId, string holderWalletUrl, string clientId, EncryptionInformation encryptionInformation, string credential, CancellationToken cancellationToken);

    Task<(string? status, string? deliveryStatus)> CheckCredentialRequestStatus(Guid companySsiDetailId, Guid externalCredentialId, string credential, CancellationToken cancellationToken);

    Task<string?> CredentialRequestAutoApprove(Guid externalCredentialId, string credential, CancellationToken cancellationToken);
}
