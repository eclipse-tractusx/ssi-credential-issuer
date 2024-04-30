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
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Services;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;

public class RevocationBusinessLogic : IRevocationBusinessLogic
{
    private readonly IIssuerRepositories _repositories;
    private readonly IWalletService _walletService;
    private readonly IIdentityData _identityData;

    public RevocationBusinessLogic(IIssuerRepositories repositories, IWalletService walletService, IIdentityService identityService)
    {
        _repositories = repositories;
        _walletService = walletService;
        _identityData = identityService.IdentityData;
    }

    public async Task RevokeCredential(Guid credentialId, bool revokeForIssuer, CancellationToken cancellationToken)
    {
        var credentialRepository = _repositories.GetInstance<ICredentialRepository>();
        var data = await credentialRepository.GetRevocationDataById(credentialId, _identityData.Bpnl)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (!data.Exists)
        {
            throw NotFoundException.Create(RevocationDataErrors.CREDENTIAL_NOT_FOUND, new ErrorParameter[] { new("credentialId", credentialId.ToString()) });
        }

        if (!revokeForIssuer && !data.IsSameBpnl)
        {
            throw ForbiddenException.Create(RevocationDataErrors.NOT_ALLOWED_TO_REVOKE_CREDENTIAL);
        }

        if (data.ExternalCredentialId is null)
        {
            throw ConflictException.Create(RevocationDataErrors.EXTERNAL_CREDENTIAL_ID_NOT_SET, new ErrorParameter[] { new("credentialId", credentialId.ToString()) });
        }

        if (data.StatusId != CompanySsiDetailStatusId.ACTIVE)
        {
            return;
        }

        // call walletService
        await _walletService.RevokeCredentialForIssuer(data.ExternalCredentialId.Value, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        _repositories.GetInstance<IDocumentRepository>().AttachAndModifyDocuments(
            data.Documents.Select(d => new ValueTuple<Guid, Action<Document>?, Action<Document>>(
                d.DocumentId,
                document => document.DocumentStatusId = d.DocumentStatusId,
                document => document.DocumentStatusId = DocumentStatusId.INACTIVE
            )));

        credentialRepository.AttachAndModifyCredential(credentialId,
            x => x.CompanySsiDetailStatusId = data.StatusId,
            x => x.CompanySsiDetailStatusId = CompanySsiDetailStatusId.REVOKED);
    }
}
