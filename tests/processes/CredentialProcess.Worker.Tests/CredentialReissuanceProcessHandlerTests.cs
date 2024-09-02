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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library;
using Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.Reissuance;
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
    private readonly ICompanySsiDetailsRepository _companySsiDetailsRepository;
    private readonly IProcessStepRepository _processStepRepository;

    public CredentialReissuanceProcessHandlerTests()
    {
        _issuerRepositories = A.Fake<IIssuerRepositories>();
        _companySsiDetailsRepository = A.Fake<ICompanySsiDetailsRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();

        A.CallTo(() => _issuerRepositories.GetInstance<ICompanySsiDetailsRepository>()).Returns(_companySsiDetailsRepository);
        A.CallTo(() => _issuerRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);

        _sut = new CredentialReissuanceProcessHandler(_issuerRepositories);
    }

    [Fact]
    public async Task RevokeReissuedCredential_WithNoCredentialFound_ThrowsConflictException()
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        A.CallTo(() => _companySsiDetailsRepository.GetCredentialToRevoke(credentialId)).Returns<Guid?>(null);
        Task Act() => _sut.RevokeReissuedCredential(credentialId);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Id of the credential to revoke should always be set here");
    }

    [Fact]
    public async Task RevokeReissuedCredential_CreateProcessStep_ReturnsExpected()
    {
        // Arrage Ids
        var credentialId = Guid.NewGuid();
        var credentialIdToRevoke = Guid.NewGuid();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.DECLINE_CREDENTIAL, Guid.NewGuid());
        var processStep = A.Fake<ProcessStep>();
        var companySsiDetail = new CompanySsiDetail(credentialIdToRevoke, "BPNL000001TEST", VerifiedCredentialTypeId.MEMBERSHIP, CompanySsiDetailStatusId.ACTIVE, "BPNL00001ISSUER", "test", DateTimeOffset.UtcNow);

        // Arrange
        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.DECLINE_CREDENTIAL)).Returns(process);
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.REVOKE_CREDENTIAL, ProcessStepStatusId.TODO, process.Id)).Returns(processStep);
        A.CallTo(() => _companySsiDetailsRepository.GetCredentialToRevoke(credentialId)).Returns(credentialIdToRevoke);
        A.CallTo(() => _companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(credentialIdToRevoke, A<Action<CompanySsiDetail>>._, A<Action<CompanySsiDetail>>._))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> modify) =>
            {
                initialize?.Invoke(companySsiDetail);
                modify(companySsiDetail);
            });

        //Act
        var result = await _sut.RevokeReissuedCredential(credentialId);

        // Assert
        companySsiDetail.ProcessId.Should().Be(process.Id);
        result.nextStepTypeIds.Should().HaveCount(1).And.Satisfy(
            x => x == ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT);
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
    }
}
