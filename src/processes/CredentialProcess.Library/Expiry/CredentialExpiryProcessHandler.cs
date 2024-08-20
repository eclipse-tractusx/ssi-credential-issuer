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
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (!data.Exists)
        {
            throw new NotFoundException($"Credential {credentialId} does not exist");
        }

        if (data.ExternalCredentialId is null)
        {
            throw new ConflictException($"External Credential Id must be set for {credentialId}");
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

        return (
            Enumerable.Repeat(ProcessStepTypeId.TRIGGER_NOTIFICATION, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerNotification(Guid credentialId, CancellationToken cancellationToken)
    {
        var (typeId, requesterId) = await _repositories.GetInstance<ICredentialRepository>().GetCredentialNotificationData(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);
        var notificationTypeId = await GetNotificationTypeId(credentialId);

        if (Guid.TryParse(requesterId, out var companyUserId))
        {
            var content = JsonSerializer.Serialize(new { Type = typeId, CredentialId = credentialId }, Options);
            await _portalService.AddNotification(content, companyUserId, notificationTypeId, cancellationToken);
        }

        return (
            Enumerable.Repeat(ProcessStepTypeId.TRIGGER_MAIL, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    private Task<NotificationTypeId> GetNotificationTypeId(Guid credentialId)
    {
        var isRevokedByReissuance = _repositories.GetInstance<IReissuanceRepository>().IsCredentialRevokedByReissuance(credentialId);
        return Task.FromResult(isRevokedByReissuance ? NotificationTypeId.CREDENTIAL_RENEWAL : NotificationTypeId.CREDENTIAL_REJECTED);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerMail(Guid credentialId, CancellationToken cancellationToken)
    {
        var (typeId, requesterId) = await _repositories.GetInstance<ICredentialRepository>().GetCredentialNotificationData(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);
        var typeValue = typeId.GetEnumValue() ?? throw new UnexpectedConditionException($"VerifiedCredentialType {typeId} does not exists");
        var isRevokedByReissuance = _repositories.GetInstance<IReissuanceRepository>().IsCredentialRevokedByReissuance(credentialId);

        if (Guid.TryParse(requesterId, out var companyUserId))
        {
            if (isRevokedByReissuance)
            {
                var mailParameters = CreateEmailParameters(typeValue, "The credential about to expiry is revoked and new credential was reissued");
                await _portalService.TriggerMail("CredentialRenewal", companyUserId, mailParameters, cancellationToken);
            }
            else
            {
                var mailParameters = CreateEmailParameters(typeValue, "The credential is already expired");
                await _portalService.TriggerMail("CredentialRejected", companyUserId, mailParameters, cancellationToken);
            }
        }

        return (
            null,
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    private static MailParameter[] CreateEmailParameters(string typeValue, string reason)
    {
        return
            [
                new("requestName", typeValue), new("reason", reason)
            ];
    }
}
