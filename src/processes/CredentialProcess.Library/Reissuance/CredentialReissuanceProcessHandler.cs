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
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.Reissuance;

public class CredentialReissuanceProcessHandler(IIssuerRepositories issuerRepositories)
    : ICredentialReissuanceProcessHandler
{
    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> RevokeReissuedCredential(Guid credentialId)
    {
        var companySsiRepository = issuerRepositories.GetInstance<ICompanySsiDetailsRepository>();
        var processStepRepository = issuerRepositories.GetInstance<IProcessStepRepository>();
        var credentialToRevokeId = await issuerRepositories.GetInstance<ICompanySsiDetailsRepository>().GetCredentialToRevoke(credentialId).ConfigureAwait(ConfigureAwaitOptions.None);

        if (credentialToRevokeId == null)
        {
            throw new ConflictException("Id of the credential to revoke should always be set here");
        }

        var processId = processStepRepository.CreateProcess(ProcessTypeId.DECLINE_CREDENTIAL).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.REVOKE_CREDENTIAL, ProcessStepStatusId.TODO, processId);
        companySsiRepository.AttachAndModifyCompanySsiDetails(credentialToRevokeId.Value, c =>
            {
                c.ProcessId = null;
            },
            c =>
            {
                c.ProcessId = processId;
            });

        return (
            Enumerable.Repeat(ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }
}

