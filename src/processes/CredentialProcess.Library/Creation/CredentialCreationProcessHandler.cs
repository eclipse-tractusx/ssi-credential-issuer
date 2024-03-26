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
using Org.Eclipse.TractusX.SsiCredentialIssuer.Callback.Service.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Callback.Service.Services;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Models;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.Creation;

public class CredentialCreationProcessHandler : ICredentialCreationProcessHandler
{
    private readonly IIssuerRepositories _issuerRepositories;
    private readonly IWalletBusinessLogic _walletBusinessLogic;
    private readonly ICallbackService _callbackService;

    public CredentialCreationProcessHandler(IIssuerRepositories issuerRepositories, IWalletBusinessLogic walletBusinessLogic, ICallbackService callbackService)
    {
        _issuerRepositories = issuerRepositories;
        _walletBusinessLogic = walletBusinessLogic;
        _callbackService = callbackService;
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateCredential(Guid credentialId, CancellationToken cancellationToken)
    {
        var data = await _issuerRepositories.GetInstance<ICredentialRepository>().GetCredentialStorageInformationById(credentialId).ConfigureAwait(false);
        await _walletBusinessLogic.CreateCredential(credentialId, data.Schema, cancellationToken).ConfigureAwait(false);
        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(ProcessStepTypeId.SIGN_CREDENTIAL, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SignCredential(Guid credentialId, CancellationToken cancellationToken)
    {
        var externalCredentialId = await _issuerRepositories.GetInstance<ICredentialRepository>().GetWalletCredentialId(credentialId).ConfigureAwait(false);
        if (externalCredentialId is null)
        {
            throw new ConflictException("ExternalCredentialId must be set here");
        }

        await _walletBusinessLogic.SignCredential(credentialId, externalCredentialId!.Value, cancellationToken).ConfigureAwait(false);
        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SaveCredentialDocument(Guid credentialId, CancellationToken cancellationToken)
    {
        var (externalCredentialId, kindId, hasEncryptionInformation, callbackUrl) = await _issuerRepositories.GetInstance<ICredentialRepository>().GetExternalCredentialAndKindId(credentialId).ConfigureAwait(false);
        if (externalCredentialId == null)
        {
            throw new ConflictException("ExternalCredentialId must be set here");
        }

        await _walletBusinessLogic.GetCredential(credentialId, externalCredentialId.Value, kindId, cancellationToken).ConfigureAwait(false);
        var nextProcessStep = callbackUrl == null ? null : Enumerable.Repeat(ProcessStepTypeId.TRIGGER_CALLBACK, 1);
        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            hasEncryptionInformation
                ? Enumerable.Repeat(ProcessStepTypeId.CREATE_CREDENTIAL_FOR_HOLDER, 1)
                : nextProcessStep,
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateCredentialForHolder(Guid credentialId, CancellationToken cancellationToken)
    {
        var (holderWalletData, credential, encryptionInformation, callbackUrl) = await _issuerRepositories.GetInstance<ICredentialRepository>().GetCredentialData(credentialId).ConfigureAwait(false);
        if (credential is null)
        {
            throw new ConflictException("Credential must be set here");
        }

        if (holderWalletData.ClientId == null || holderWalletData.WalletUrl == null)
        {
            throw new ConflictException("Wallet information must be set");
        }

        if (encryptionInformation.Secret == null || encryptionInformation.InitializationVector == null || encryptionInformation.EncryptionMode == null)
        {
            throw new ConflictException("Wallet secret must be set");
        }

        await _walletBusinessLogic.CreateCredentialForHolder(credentialId, holderWalletData.WalletUrl, holderWalletData.ClientId, new EncryptionInformation(encryptionInformation.Secret, encryptionInformation.InitializationVector, encryptionInformation.EncryptionMode.Value), credential, cancellationToken).ConfigureAwait(false);
        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            callbackUrl is null ? null : Enumerable.Repeat(ProcessStepTypeId.TRIGGER_CALLBACK, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerCallback(Guid credentialId, CancellationToken cancellationToken)
    {
        var (bpn, callbackUrl) = await _issuerRepositories.GetInstance<ICredentialRepository>().GetCallbackUrl(credentialId).ConfigureAwait(false);
        if (callbackUrl is null)
        {
            throw new ConflictException("CallbackUrl must be set");
        }

        var issuerResponseData = new IssuerResponseData(bpn, IssuerResponseStatus.SUCCESSFUL, "Successfully created Credential");
        await _callbackService.TriggerCallback(callbackUrl, issuerResponseData, cancellationToken).ConfigureAwait(false);
        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            null,
            ProcessStepStatusId.DONE,
            false,
            null);
    }
}
