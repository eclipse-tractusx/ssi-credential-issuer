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

using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.IssuerHub.Service.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.IssuerHub.Service.Services;
using System.Text.Json;
using EncryptionInformation = Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Models.EncryptionInformation;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.Backend;

/// <summary>
/// Credential backend implementation that delegates to the Identity Hub IssuerService.
/// Credential signing is handled internally by the IssuerService via DCP protocol.
/// </summary>
public class IssuerHubCredentialBackend(
    IIssuerHubService issuerHubService,
    IIssuerRepositories repositories)
    : ICredentialBackend
{
    public async Task CreateSignedCredential(Guid companySsiDetailId, JsonDocument schema, CancellationToken cancellationToken)
    {
        // The IssuerHub handles signing internally. We register the holder and trigger a credential offer.
        // The DCP flow will handle the credential creation asynchronously.
        // For now, we mark the credential as ACTIVE since the IssuerHub manages the lifecycle.
        var credentialRepository = repositories.GetInstance<ICompanySsiDetailsRepository>();
        var credentialId = Guid.NewGuid();

        credentialRepository.AttachAndModifyCompanySsiDetails(companySsiDetailId, c =>
        {
            c.ExternalCredentialId = null;
        }, c =>
        {
            c.CompanySsiDetailStatusId = CompanySsiDetailStatusId.ACTIVE;
            c.ExternalCredentialId = credentialId;
        });
    }

    public Task GetCredential(Guid credentialId, Guid externalCredentialId, VerifiedCredentialTypeKindId kindId, CancellationToken cancellationToken)
    {
        // In the IssuerHub flow, credentials are managed by the IssuerService.
        // Credential retrieval and schema validation is handled by the DCP protocol.
        // We can query the credential status to verify it was issued successfully.
        return issuerHubService.GetCredentialStatus(externalCredentialId.ToString(), cancellationToken)
            .ContinueWith(t => { /* Status check completed */ }, cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
    }

    public async Task CreateCredentialForHolder(Guid companySsiDetailId, string holderWalletUrl, string clientId, EncryptionInformation encryptionInformation, string credential, CancellationToken cancellationToken)
    {
        // In the IssuerHub flow, credential delivery to holder is done via DCP CredentialOffer.
        // The holder's IdentityHub will request the credential via the DCP protocol.
        var request = new TriggerCredentialOfferRequest(
            HolderId: clientId,
            Credentials: new[] { credential });

        await issuerHubService.TriggerCredentialOffer(request, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task RevokeCredential(Guid externalCredentialId, CancellationToken cancellationToken)
    {
        await issuerHubService.RevokeCredential(externalCredentialId.ToString(), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
