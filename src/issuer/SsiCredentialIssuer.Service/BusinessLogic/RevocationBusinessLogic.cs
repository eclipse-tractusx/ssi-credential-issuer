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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Services;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;

public class RevocationBusinessLogic : IRevocationBusinessLogic
{
    private readonly IIssuerRepositories _repositories;
    private readonly IWalletService _walletService;

    public RevocationBusinessLogic(IIssuerRepositories repositories, IWalletService walletService)
    {
        _repositories = repositories;
        _walletService = walletService;
    }

    public async Task RevokeIssuerCredential(Guid credentialId, CancellationToken cancellationToken)
    {
        // check for is issuer
        var credentialRepository = _repositories.GetInstance<ICredentialRepository>();
        var data = await RevokeCredentialInternal(credentialId, credentialRepository).ConfigureAwait(false);
        if (data.StatusId != CompanySsiDetailStatusId.ACTIVE)
        {
            return;
        }

        // call walletService
        await _walletService.RevokeCredentialForIssuer(data.ExternalCredentialId, cancellationToken).ConfigureAwait(false);
        UpdateData(credentialId, data.StatusId, data.Documents, credentialRepository);
    }

    public async Task RevokeHolderCredential(Guid credentialId, TechnicalUserDetails walletInformation, CancellationToken cancellationToken)
    {
        // check for is holder
        var credentialRepository = _repositories.GetInstance<ICredentialRepository>();
        var data = await RevokeCredentialInternal(credentialId, credentialRepository).ConfigureAwait(false);
        if (data.StatusId != CompanySsiDetailStatusId.ACTIVE)
        {
            return;
        }

        // call walletService
        await _walletService.RevokeCredentialForHolder(walletInformation.WalletUrl, walletInformation.ClientId, walletInformation.ClientSecret, data.ExternalCredentialId, cancellationToken).ConfigureAwait(false);
        UpdateData(credentialId, data.StatusId, data.Documents, credentialRepository);
    }

    private static async Task<(Guid ExternalCredentialId, CompanySsiDetailStatusId StatusId, IEnumerable<(Guid DocumentId, DocumentStatusId DocumentStatusId)> Documents)> RevokeCredentialInternal(Guid credentialId, ICredentialRepository credentialRepository)
    {
        var data = await credentialRepository.GetRevocationDataById(credentialId)
            .ConfigureAwait(false);
        if (!data.Exists)
        {
            throw NotFoundException.Create(RevocationDataErrors.CREDENTIAL_NOT_FOUND, new ErrorParameter[] { new("credentialId", credentialId.ToString()) });
        }

        if (data.ExternalCredentialId is null)
        {
            throw ConflictException.Create(RevocationDataErrors.EXTERNAL_CREDENTIAL_ID_NOT_SET, new ErrorParameter[] { new("credentialId", credentialId.ToString()) });
        }

        return (data.ExternalCredentialId.Value, data.StatusId, data.Documents);
    }

    private void UpdateData(Guid credentialId, CompanySsiDetailStatusId statusId, IEnumerable<(Guid DocumentId, DocumentStatusId DocumentStatusId)> documentData, ICredentialRepository credentialRepository)
    {
        _repositories.GetInstance<IDocumentRepository>().AttachAndModifyDocuments(
            documentData.Select(d => new ValueTuple<Guid, Action<Document>?, Action<Document>>(
                d.DocumentId,
                document => document.DocumentStatusId = d.DocumentStatusId,
                document => document.DocumentStatusId = DocumentStatusId.INACTIVE
            )));

        credentialRepository.AttachAndModifyCredential(credentialId,
            x => x.CompanySsiDetailStatusId = statusId,
            x => x.CompanySsiDetailStatusId = CompanySsiDetailStatusId.REVOKED);
    }
}
