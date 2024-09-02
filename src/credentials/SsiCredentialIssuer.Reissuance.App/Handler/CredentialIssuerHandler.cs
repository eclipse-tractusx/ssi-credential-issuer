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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.DependencyInjection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Handler;

/// <inheritdoc />
public class CredentialIssuerHandler(IIssuerRepositories repositories, IOptions<ReissuanceSettings> options) : ICredentialIssuerHandler
{
    private readonly ReissuanceSettings _settings = options.Value;

    /// <inheritdoc />
    public async Task HandleCredentialProcessCreation(IssuerCredentialRequest issuerCredentialRequest)
    {
        var documentContent = Encoding.UTF8.GetBytes(issuerCredentialRequest.Schema);
        var hash = SHA512.HashData(documentContent);
        var documentRepository = repositories.GetInstance<IDocumentRepository>();
        var companyCredentialDetailsRepository = repositories.GetInstance<ICompanySsiDetailsRepository>();
        var docId = documentRepository.CreateDocument($"{issuerCredentialRequest.TypeId}.json", documentContent,
            hash, MediaTypeId.JSON, DocumentTypeId.PRESENTATION, x =>
            {
                x.IdentityId = issuerCredentialRequest.IdentiyId;
                x.DocumentStatusId = DocumentStatusId.ACTIVE;
            }).Id;

        Guid? processId = CreateProcess(repositories);

        var ssiDetailId = companyCredentialDetailsRepository.CreateSsiDetails(
            issuerCredentialRequest.Bpnl,
            issuerCredentialRequest.TypeId,
            CompanySsiDetailStatusId.ACTIVE,
            _settings.IssuerBpn,
            issuerCredentialRequest.IdentiyId,
            c =>
            {
                c.VerifiedCredentialExternalTypeDetailVersionId = issuerCredentialRequest.DetailVersionId;
                c.ProcessId = processId;
                c.ExpiryDate = issuerCredentialRequest.ExpiryDate;
                c.ReissuedCredentialId = issuerCredentialRequest.Id;
            }).Id;

        documentRepository.AssignDocumentToCompanySsiDetails(docId, ssiDetailId);

        companyCredentialDetailsRepository.CreateProcessData(ssiDetailId, JsonDocument.Parse(issuerCredentialRequest.Schema), issuerCredentialRequest.KindId,
            c =>
            {
                c.HolderWalletUrl = issuerCredentialRequest.HolderWalletUrl;
                c.CallbackUrl = issuerCredentialRequest.CallbackUrl;
            });

        await repositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static Guid CreateProcess(IIssuerRepositories repositories)
    {
        var processStepRepository = repositories.GetInstance<IProcessStepRepository>();
        var processId = processStepRepository.CreateProcess(ProcessTypeId.CREATE_CREDENTIAL).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_CREDENTIAL, ProcessStepStatusId.TODO, processId);
        return processId;
    }
}
