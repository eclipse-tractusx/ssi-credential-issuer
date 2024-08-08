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

using Microsoft.Extensions.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Credential.Library.Configuration;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Renewal.App.Handlers;

/// <summary>
/// Handles the re-issuance of a new credential then creates a new create credential process 
/// </summary>
public class CredentialIssuerHandler : ICredentialIssuerHandler
{
    private readonly CredentialSettings _settings;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="serviceScopeFactory"></param>
    /// <param name="options"></param>
    public CredentialIssuerHandler(IServiceScopeFactory serviceScopeFactory, IOptions<CredentialSettings> options)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _settings = options.Value;
    }

    /// <summary>
    /// Hadkes the request to create a new credential process
    /// </summary>
    /// <param name="credentialRequest">Credential Request Object</param>
    /// <returns></returns>
    public async Task HandleCredentialProcessCreation(IssuerCredentialRequest credentialRequest)
    {
        var documentContent = Encoding.UTF8.GetBytes(credentialRequest.Schema);
        var hash = SHA512.HashData(documentContent);
        using var processServiceScope = _serviceScopeFactory.CreateScope();
        var repositories = processServiceScope.ServiceProvider.GetRequiredService<IIssuerRepositories>();
        var documentRepository = repositories.GetInstance<IDocumentRepository>();
        var companyCredentialDetailsRepository = repositories.GetInstance<ICompanySsiDetailsRepository>();
        var docId = documentRepository.CreateDocument($"{credentialRequest.TypeId}.json", documentContent,
            hash, MediaTypeId.JSON, DocumentTypeId.PRESENTATION, x =>
            {
                x.IdentityId = credentialRequest.IdentiyId;
                x.DocumentStatusId = DocumentStatusId.ACTIVE;
            }).Id;

        Guid? processId = CreateProcess(repositories);

        var ssiDetailId = companyCredentialDetailsRepository.CreateSsiDetails(
            credentialRequest.Bpnl,
            credentialRequest.TypeId,
            CompanySsiDetailStatusId.ACTIVE,
            _settings.IssuerBpn,
            credentialRequest.IdentiyId,
            c =>
            {
                c.VerifiedCredentialExternalTypeDetailVersionId = credentialRequest.DetailVersionId;
                c.ProcessId = processId;
                c.ExpiryDate = credentialRequest.ExpiryDate;
            }).Id;
        documentRepository.AssignDocumentToCompanySsiDetails(docId, ssiDetailId);

        companyCredentialDetailsRepository.CreateProcessData(ssiDetailId, JsonDocument.Parse(credentialRequest.Schema), credentialRequest.KindId,
            c =>
            {
                c.HolderWalletUrl = credentialRequest.HolderWalletUrl;
                c.CallbackUrl = credentialRequest.CallbackUrl;
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
