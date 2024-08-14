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

using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Worker.Tests;

public class CredentialReissuanceProcessHandlerTests
{
    private readonly ICredentialReissuanceProcessHandler _sut;
    private readonly IIssuerRepositories _issuerRepositories;
    private readonly IReissuanceRepository _reissuanceRepository;
    private readonly ICompanySsiDetailsRepository _companySsiDetailsRepository;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly ILogger<CredentialReissuanceProcessHandler> _logger;

    public CredentialReissuanceProcessHandlerTests()
    {
        _issuerRepositories = A.Fake<IIssuerRepositories>();
        _reissuanceRepository = A.Fake<IReissuanceRepository>();
        _companySsiDetailsRepository = A.Fake<ICompanySsiDetailsRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();

        A.CallTo(() => _issuerRepositories.GetInstance<IReissuanceRepository>()).Returns(_reissuanceRepository);
        A.CallTo(() => _issuerRepositories.GetInstance<ICompanySsiDetailsRepository>()).Returns(_companySsiDetailsRepository);
        A.CallTo(() => _issuerRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);

        _logger = A.Fake<ILogger<CredentialReissuanceProcessHandler>>();
        _sut = new CredentialReissuanceProcessHandler(_issuerRepositories, _logger);
    }

    [Fact]
    public void RevokeReissuedCredential_isReissuedCredentialFalse_ReturnsExpected()
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        A.CallTo(() => _issuerRepositories.GetInstance<IReissuanceRepository>().IsReissuedCredential(credentialId)).Returns(false);

        // Act
        var revokeCredentialResponse = _sut.RevokeReissuedCredential(credentialId);

        // Assert
        AssertSuccessResult(revokeCredentialResponse);
    }

    [Fact]
    public void RevokeReissuedCredential_CreateProcessStepThrowsException_ReturnsExpected()
    {
        // Arrage Ids
        var credentialId = Guid.NewGuid();
        var process = new Entities.Entities.Process(Guid.NewGuid(), ProcessTypeId.DECLINE_CREDENTIAL, Guid.NewGuid());

        // Arrange
        A.CallTo(() => _issuerRepositories.GetInstance<IReissuanceRepository>().IsReissuedCredential(credentialId)).Returns(true);
        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.DECLINE_CREDENTIAL)).Returns(process);
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.REVOKE_CREDENTIAL, ProcessStepStatusId.TODO, process.Id))
            .Throws(new Exception("not possible to create step process exception"));

        //Act
        var revokeCredentialResponse = _sut.RevokeReissuedCredential(credentialId);

        // Assert
        AssertSuccessResult(revokeCredentialResponse);
    }

    [Fact]
    public void RevokeReissuedCredential_CreateProcessStep_ReturnsExpected()
    {
        // Arrage Ids
        var credentialId = Guid.NewGuid();
        var process = new Entities.Entities.Process(Guid.NewGuid(), ProcessTypeId.DECLINE_CREDENTIAL, Guid.NewGuid());
        var processStep = A.Fake<ProcessStep>();

        // Arrange
        A.CallTo(() => _issuerRepositories.GetInstance<IReissuanceRepository>().IsReissuedCredential(credentialId)).Returns(true);
        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.DECLINE_CREDENTIAL)).Returns(process);
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.REVOKE_CREDENTIAL, ProcessStepStatusId.TODO, process.Id)).Returns(processStep);
        A.CallTo(() => _reissuanceRepository.GetCompanySsiDetailId(credentialId)).Returns(credentialId);

        //Act
        var revokeCredentialResponse = _sut.RevokeReissuedCredential(credentialId);

        // Assert
        A.CallTo(() => _companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(credentialId, null, null)).WithAnyArguments().MustHaveHappened();
        AssertSuccessResult(revokeCredentialResponse);
    }

    private static void AssertSuccessResult(Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> revokeCredentialResponse)
    {
        var result = revokeCredentialResponse.Result;
        result.nextStepTypeIds.Should().HaveCount(1).And.Satisfy(
            x => x == ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT);
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
    }
}
