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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Services;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Services;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.Expiry;

public class CredentialExpiryProcessHandler : ICredentialExpiryProcessHandler
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IIssuerRepositories _repositories;
    private readonly IWalletService _walletService;
    private readonly IPortalService _portalService;

    public CredentialExpiryProcessHandler(IIssuerRepositories repositories, IWalletService walletService, IPortalService portalService)
    {
        _repositories = repositories;
        _walletService = walletService;
        _portalService = portalService;
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> RevokeCredential(Guid credentialId, CancellationToken cancellationToken)
    {
        var credentialRepository = _repositories.GetInstance<ICredentialRepository>();
        var data = await credentialRepository.GetRevocationDataById(credentialId, string.Empty)
            .ConfigureAwait(false);
        if (!data.Exists)
        {
            throw new NotFoundException($"Credential {credentialId} does not exist");
        }

        if (data.ExternalCredentialId is null)
        {
            throw new ConflictException($"External Credential Id must be set for {credentialId}");
        }

        // call walletService
        await _walletService.RevokeCredentialForIssuer(data.ExternalCredentialId.Value, cancellationToken).ConfigureAwait(false);

        _repositories.GetInstance<IDocumentRepository>().AttachAndModifyDocuments(
            data.Documents.Select(d => new ValueTuple<Guid, Action<Document>?, Action<Document>>(
                d.DocumentId,
                document => document.DocumentStatusId = d.DocumentStatusId,
                document => document.DocumentStatusId = DocumentStatusId.INACTIVE
            )));

        credentialRepository.AttachAndModifyCredential(credentialId,
            x => x.CompanySsiDetailStatusId = data.StatusId,
            x => x.CompanySsiDetailStatusId = CompanySsiDetailStatusId.REVOKED);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(ProcessStepTypeId.TRIGGER_NOTIFICATION, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerNotification(Guid credentialId, CancellationToken cancellationToken)
    {
        var (typeId, requesterId) = await _repositories.GetInstance<ICredentialRepository>().GetCredentialNotificationData(credentialId).ConfigureAwait(false);
        if (Guid.TryParse(requesterId, out var companyUserId))
        {
            var content = JsonSerializer.Serialize(new { Type = typeId, CredentialId = credentialId }, Options);
            await _portalService.AddNotification(content, companyUserId, NotificationTypeId.CREDENTIAL_REJECTED, cancellationToken);
        }

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(ProcessStepTypeId.TRIGGER_MAIL, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerMail(Guid credentialId, CancellationToken cancellationToken)
    {
        var (typeId, requesterId) = await _repositories.GetInstance<ICredentialRepository>().GetCredentialNotificationData(credentialId).ConfigureAwait(false);

        var typeValue = typeId.GetEnumValue() ?? throw new UnexpectedConditionException($"VerifiedCredentialType {typeId} does not exists");
        if (Guid.TryParse(requesterId, out var companyUserId))
        {
            var mailParameters = new MailParameter[]
            {
                new("requestName", typeValue), new("reason", "The credential is already expired")
            };
            await _portalService.TriggerMail("CredentialRejected", companyUserId, mailParameters, cancellationToken);
        }

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            null,
            ProcessStepStatusId.DONE,
            false,
            null);
    }
}
