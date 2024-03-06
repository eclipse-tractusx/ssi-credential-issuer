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
using Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Processes.Worker.Library;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Worker;

public class CredentialProcessTypeExecutor : IProcessTypeExecutor
{
    private readonly IIssuerRepositories _issuerRepositories;
    private readonly ICredentialProcessHandler _credentialProcessHandler;

    private readonly IEnumerable<ProcessStepTypeId> _executableProcessSteps = ImmutableArray.Create(
        ProcessStepTypeId.CREATE_CREDENTIAL,
        ProcessStepTypeId.SIGN_CREDENTIAL,
        ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT,
        ProcessStepTypeId.CREATE_CREDENTIAL_FOR_HOLDER);

    private Guid _credentialId;

    public CredentialProcessTypeExecutor(
        IIssuerRepositories issuerRepositories,
        ICredentialProcessHandler credentialProcessHandler)
    {
        _issuerRepositories = issuerRepositories;
        _credentialProcessHandler = credentialProcessHandler;
    }

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.CREATE_CREDENTIAL;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => _executableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => _executableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => new(false);

    public async ValueTask<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        var (exists, credentialId) = await _issuerRepositories.GetInstance<ICredentialRepository>().GetDataForProcessId(processId).ConfigureAwait(false);
        if (!exists)
        {
            throw new NotFoundException($"process {processId} does not exist or is not associated with an credential");
        }

        _credentialId = credentialId;
        return new IProcessTypeExecutor.InitializationResult(false, null);
    }

    public async ValueTask<IProcessTypeExecutor.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
    {
        if (_credentialId == Guid.Empty)
        {
            throw new UnexpectedConditionException("credentialId should never be empty here");
        }

        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        ProcessStepStatusId stepStatusId;
        bool modified;
        string? processMessage;

        try
        {
            (nextStepTypeIds, stepStatusId, modified, processMessage) = processStepTypeId switch
            {
                ProcessStepTypeId.CREATE_CREDENTIAL => await _credentialProcessHandler.CreateCredential(_credentialId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.SIGN_CREDENTIAL => await _credentialProcessHandler.SignCredential(_credentialId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT => await _credentialProcessHandler.SaveCredentialDocument(_credentialId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.CREATE_CREDENTIAL_FOR_HOLDER => await _credentialProcessHandler.CreateCredentialForHolder(_credentialId, cancellationToken)
                    .ConfigureAwait(false),
                _ => (null, ProcessStepStatusId.TODO, false, null)
            };
        }
        catch (Exception ex) when (ex is not SystemException)
        {
            (stepStatusId, processMessage, nextStepTypeIds) = ProcessError(ex);
            modified = true;
        }

        return new IProcessTypeExecutor.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, null, processMessage);
    }

    private static (ProcessStepStatusId StatusId, string? ProcessMessage, IEnumerable<ProcessStepTypeId>? nextSteps) ProcessError(Exception ex)
    {
        return ex switch
        {
            ServiceException { IsRecoverable: true } => (ProcessStepStatusId.TODO, ex.Message, null),
            _ => (ProcessStepStatusId.FAILED, ex.Message, null)
        };
    }
}
