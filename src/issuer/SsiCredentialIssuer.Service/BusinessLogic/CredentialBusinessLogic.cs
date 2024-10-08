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
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Extensions;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;

public class CredentialBusinessLogic(IIssuerRepositories repositories, IIdentityService identityService)
    : ICredentialBusinessLogic
{
    private readonly IIdentityData _identityData = identityService.IdentityData;

    public async Task<JsonDocument> GetCredentialDocument(Guid credentialId)
    {
        var (exists, isSameCompany, documents) = await repositories.GetInstance<ICredentialRepository>().GetSignedCredentialForCredentialId(credentialId, _identityData.Bpnl).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!exists)
        {
            throw NotFoundException.Create(CredentialErrors.CREDENTIAL_NOT_FOUND, new[] { new ErrorParameter("credentialId", credentialId.ToString()) });
        }

        if (!isSameCompany)
        {
            throw ForbiddenException.Create(CredentialErrors.COMPANY_NOT_ALLOWED);
        }

        if (documents.Count() != 1)
        {
            throw ConflictException.Create(CredentialErrors.SIGNED_CREDENTIAL_NOT_FOUND);
        }

        var (_, credentialContent) = documents.Single();
        using var stream = new MemoryStream(credentialContent);
        return await JsonDocument.ParseAsync(stream).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<(string FileName, byte[] Content, string MediaType)> GetCredentialDocumentById(Guid documentId)
    {
        var (exists, isSameCompany, fileName, documentStatusId, content, mediaTypeId) = await repositories.GetInstance<ICredentialRepository>().GetDocumentById(documentId, _identityData.Bpnl).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!exists)
        {
            throw NotFoundException.Create(CredentialErrors.DOCUMENT_NOT_FOUND, new[] { new ErrorParameter("documentId", documentId.ToString()) });
        }

        if (!isSameCompany)
        {
            throw ForbiddenException.Create(CredentialErrors.DOCUMENT_OTHER_COMPANY);
        }

        if (documentStatusId == DocumentStatusId.INACTIVE)
        {
            throw ConflictException.Create(CredentialErrors.DOCUMENT_INACTIVE, new[] { new ErrorParameter("documentId", documentId.ToString()) });
        }

        return (fileName, content, mediaTypeId.MapToMediaType());
    }
}
