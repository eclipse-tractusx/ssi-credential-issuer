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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Callback.Service.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Callback.Service.Services;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.BusinessLogic;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.Creation;

public class CredentialCreationProcessHandler(
    IIssuerRepositories issuerRepositories,
    IWalletBusinessLogic walletBusinessLogic,
    ICallbackService callbackService)
    : ICredentialCreationProcessHandler
{
    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateSignedCredential(Guid credentialId, CancellationToken cancellationToken)
    {
        var data = await issuerRepositories.GetInstance<ICredentialRepository>().GetCredentialStorageInformationById(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);
        await walletBusinessLogic.CreateSignedCredential(credentialId, data.Schema, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return (
            Enumerable.Repeat(ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SaveCredentialDocument(Guid credentialId, CancellationToken cancellationToken)
    {
        var (externalCredentialId, kindId, _, _) = await issuerRepositories.GetInstance<ICredentialRepository>().GetExternalCredentialAndKindId(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (externalCredentialId == null)
        {
            throw new ConflictException("ExternalCredentialId must be set here");
        }

        await walletBusinessLogic.GetCredential(credentialId, externalCredentialId.Value, kindId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        return (
            Enumerable.Repeat(ProcessStepTypeId.OFFER_CREDENTIAL_TO_HOLDER, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> OfferCredentialToHolder(Guid credentialId, CancellationToken cancellationToken)
    {
        var (isIssuerCompany, externalCredentialId, credentialJson, callbackUrl) = await issuerRepositories.GetInstance<ICredentialRepository>().GetCredentialById(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (isIssuerCompany)
        {
            return (
                callbackUrl is null ? null : Enumerable.Repeat(ProcessStepTypeId.TRIGGER_CALLBACK, 1),
                ProcessStepStatusId.SKIPPED,
                false,
                "ProcessStep was skipped because the holder is the issuer");
        }

        if (credentialJson is null)
        {
            throw new ConflictException("Credential json must be set here");
        }
        if (externalCredentialId == null)
        {
            throw new ConflictException("ExternalCredentialId must be set here");
        }

        await walletBusinessLogic.OfferCredentialToHolder(externalCredentialId.Value, credentialJson.RootElement.GetRawText(), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return (
            callbackUrl is null ? null : Enumerable.Repeat(ProcessStepTypeId.TRIGGER_CALLBACK, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerCallback(Guid credentialId, CancellationToken cancellationToken)
    {
        var (bpn, callbackUrl) = await issuerRepositories.GetInstance<ICredentialRepository>().GetCallbackUrl(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (callbackUrl is null)
        {
            throw new ConflictException("CallbackUrl must be set");
        }

        var issuerResponseData = new IssuerResponseData(bpn, IssuerResponseStatus.SUCCESSFUL, "Successfully created Credential");
        await callbackService.TriggerCallback(callbackUrl, issuerResponseData, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return (
            null,
            ProcessStepStatusId.DONE,
            false,
            null);
    }
}
