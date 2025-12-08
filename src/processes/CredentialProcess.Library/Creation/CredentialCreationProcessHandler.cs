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
using Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Models;

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
        var (externalCredentialId, kindId, hasEncryptionInformation, _) = await issuerRepositories.GetInstance<ICredentialRepository>().GetExternalCredentialAndKindId(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (externalCredentialId == null)
        {
            throw ConflictException.Create(CredentialProcessErrors.EXTERNAL_CREDENTIAL_ID_NOT_SET);
        }

        await walletBusinessLogic.GetCredential(credentialId, externalCredentialId.Value, kindId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return (
            hasEncryptionInformation
                ? Enumerable.Repeat(ProcessStepTypeId.REQUEST_CREDENTIAL_FOR_HOLDER, 1)
                : Enumerable.Repeat(ProcessStepTypeId.REQUEST_CREDENTIAL_AUTO_APPROVE, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateCredentialForHolder(Guid credentialId, CancellationToken cancellationToken)
    {
        var (isIssuerCompany, holderWalletData, credential, _, encryptionInformation, callbackUrl) = await issuerRepositories.GetInstance<ICredentialRepository>().GetCredentialData(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (isIssuerCompany)
        {
            return (
                callbackUrl is null ? null : Enumerable.Repeat(ProcessStepTypeId.TRIGGER_CALLBACK, 1),
                ProcessStepStatusId.SKIPPED,
                false,
                "ProcessStep was skipped because the holder is the issuer");
        }

        if (credential is null)
        {
            throw ConflictException.Create(CredentialProcessErrors.CREDENTIAL_NOT_SET);
        }

        if (holderWalletData.ClientId == null || holderWalletData.WalletUrl == null)
        {
            throw ConflictException.Create(CredentialProcessErrors.WALLET_INFO_NOT_SET);
        }

        if (encryptionInformation.Secret == null || encryptionInformation.InitializationVector == null || encryptionInformation.EncryptionMode == null)
        {
            throw ConflictException.Create(CredentialProcessErrors.WALLET_SECRET_NOT_SET);
        }

        await walletBusinessLogic.CreateCredentialForHolder(credentialId, holderWalletData.WalletUrl, holderWalletData.ClientId, new EncryptionInformation(encryptionInformation.Secret, encryptionInformation.InitializationVector, encryptionInformation.EncryptionMode.Value), credential, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return (
            callbackUrl is null ? null : Enumerable.Repeat(ProcessStepTypeId.TRIGGER_CALLBACK, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> RequestCredentialForHolder(Guid credentialId, CancellationToken cancellationToken)
    {
        var (isIssuerCompany, holderWalletData, _, credentialJson, encryptionInformation, callbackUrl) = await issuerRepositories.GetInstance<ICredentialRepository>().GetCredentialData(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);
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
            throw ConflictException.Create(CredentialProcessErrors.CREDENTIAL_NOT_SET);
        }

        if (holderWalletData.ClientId == null || holderWalletData.WalletUrl == null)
        {
            return (
                Enumerable.Repeat(ProcessStepTypeId.REQUEST_CREDENTIAL_STATUS_CHECK, 1),
                ProcessStepStatusId.SKIPPED,
                false,
                "ProcessStep was skipped because the holder is the BYOW");
        }

        if (encryptionInformation.Secret == null || encryptionInformation.InitializationVector == null || encryptionInformation.EncryptionMode == null)
        {
            throw ConflictException.Create(CredentialProcessErrors.WALLET_SECRET_NOT_SET);
        }

        await walletBusinessLogic.RequestCredentialForHolder(credentialId, holderWalletData.WalletUrl, holderWalletData.ClientId, new EncryptionInformation(encryptionInformation.Secret, encryptionInformation.InitializationVector, encryptionInformation.EncryptionMode.Value), credentialJson.RootElement.GetRawText(), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return (
            Enumerable.Repeat(ProcessStepTypeId.REQUEST_CREDENTIAL_AUTO_APPROVE, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> RequestCredentialAutoApprove(Guid credentialId, CancellationToken cancellationToken)
    {
        var (externalCredentialId, credentialJson, _) = await issuerRepositories.GetInstance<ICredentialRepository>().GetCredentialDetailById(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (externalCredentialId == null)
        {
            throw ConflictException.Create(CredentialProcessErrors.EXTERNAL_CREDENTIAL_ID_NOT_SET);
        }
        if (credentialJson is null)
        {
            throw ConflictException.Create(CredentialProcessErrors.CREDENTIAL_NOT_SET);
        }

        var credentialRequestStatus = await walletBusinessLogic.CredentialRequestAutoApprove(externalCredentialId.Value, credentialJson.RootElement.GetRawText(), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        if (credentialRequestStatus == "successful")
        {
            return (
                Enumerable.Repeat(ProcessStepTypeId.REQUEST_CREDENTIAL_STATUS_CHECK, 1),
                ProcessStepStatusId.DONE,
                false,
                null);
        }
        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
                null,
                ProcessStepStatusId.TODO,
                false,
                null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CheckCredentialStatus(Guid credentialId, CancellationToken cancellationToken)
    {
        var (externalCredentialId, credentialJson, callbackUrl) = await issuerRepositories.GetInstance<ICredentialRepository>().GetCredentialDetailById(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (externalCredentialId == null)
        {
            throw ConflictException.Create(CredentialProcessErrors.EXTERNAL_CREDENTIAL_ID_NOT_SET);
        }
        if (credentialJson is null)
        {
            throw ConflictException.Create(CredentialProcessErrors.CREDENTIAL_NOT_SET);
        }

        var (credentialRequestStatus, deliveryStatus) = await walletBusinessLogic.CheckCredentialRequestStatus(credentialId, externalCredentialId.Value, credentialJson.RootElement.GetRawText(), cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        if (credentialRequestStatus == "ISSUED" && deliveryStatus == "COMPLETED")
        {
            return (
            callbackUrl == null ? null : Enumerable.Repeat(ProcessStepTypeId.TRIGGER_CALLBACK, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
        }
        if (credentialRequestStatus == "ISSUED" && deliveryStatus == "FAILED")
        {
            return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
                Enumerable.Repeat(ProcessStepTypeId.REQUEST_CREDENTIAL_AUTO_APPROVE, 1),
                ProcessStepStatusId.SKIPPED,
                false,
                null);
        }
        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
                null,
                ProcessStepStatusId.TODO,
                false,
                null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> TriggerCallback(Guid credentialId, CancellationToken cancellationToken)
    {
        var (bpn, callbackUrl) = await issuerRepositories.GetInstance<ICredentialRepository>().GetCallbackUrl(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (callbackUrl is null)
        {
            throw ConflictException.Create(CredentialProcessErrors.CALLBACK_URL_NOT_SET);
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
