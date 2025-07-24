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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.Creation;
using Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Worker.Creation;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Worker.Tests;

public class CredentialCreationProcessTypeExecutorTests
{
    private readonly CredentialCreationProcessTypeExecutor _sut;
    private readonly ICredentialCreationProcessHandler _credentialCreationProcessHandler;
    private readonly ICredentialRepository _credentialRepository;

    public CredentialCreationProcessTypeExecutorTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var issuerRepositories = A.Fake<IIssuerRepositories>();
        _credentialCreationProcessHandler = A.Fake<ICredentialCreationProcessHandler>();

        _credentialRepository = A.Fake<ICredentialRepository>();

        A.CallTo(() => issuerRepositories.GetInstance<ICredentialRepository>()).Returns(_credentialRepository);

        _sut = new CredentialCreationProcessTypeExecutor(issuerRepositories, _credentialCreationProcessHandler);
    }

    [Fact]
    public void GetProcessTypeId_ReturnsExpected()
    {
        // Assert
        _sut.GetProcessTypeId().Should().Be(ProcessTypeId.CREATE_CREDENTIAL);
    }

    [Fact]
    public void IsExecutableStepTypeId_WithValid_ReturnsExpected()
    {
        // Assert
        _sut.IsExecutableStepTypeId(ProcessStepTypeId.CREATE_SIGNED_CREDENTIAL).Should().BeTrue();
    }

    [Fact]
    public void GetExecutableStepTypeIds_ReturnsExpected()
    {
        // Assert
        _sut.GetExecutableStepTypeIds().Should().HaveCount(6).And.Satisfy(
            x => x == ProcessStepTypeId.CREATE_SIGNED_CREDENTIAL,
            x => x == ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT,
            x => x == ProcessStepTypeId.REQUEST_CREDENTIAL_FOR_HOLDER,
            x => x == ProcessStepTypeId.REQUEST_CREDENTIAL_AUTO_APPROVE,
            x => x == ProcessStepTypeId.REQUEST_CREDENTIAL_STATUS_CHECK,
            x => x == ProcessStepTypeId.TRIGGER_CALLBACK);
    }

    [Fact]
    public async Task IsLockRequested_ReturnsExpected()
    {
        // Act
        var result = await _sut.IsLockRequested(ProcessStepTypeId.CREATE_SIGNED_CREDENTIAL);

        // Assert
        result.Should().BeFalse();
    }

    #region InitializeProcess

    [Fact]
    public async Task InitializeProcess_WithExistingProcess_ReturnsExpected()
    {
        // Arrange
        var validProcessId = Guid.NewGuid();
        A.CallTo(() => _credentialRepository.GetDataForProcessId(validProcessId))
            .Returns((true, Guid.NewGuid()));

        // Act
        var result = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task InitializeProcess_WithNotExistingProcess_ThrowsNotFoundException()
    {
        // Arrange
        var validProcessId = Guid.NewGuid();
        A.CallTo(() => _credentialRepository.GetDataForProcessId(validProcessId))
            .Returns(default((bool, Guid)));

        // Act
        async Task Act() => await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"process {validProcessId} does not exist or is not associated with an credential");
    }

    #endregion

    #region ExecuteProcessStep

    [Fact]
    public async Task ExecuteProcessStep_WithoutRegistrationId_ThrowsUnexpectedConditionException()
    {
        // Act
        async Task Act() => await _sut.ExecuteProcessStep(ProcessStepTypeId.CREATE_SIGNED_CREDENTIAL, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be("credentialId should never be empty here");
    }

    [Fact]
    public async Task ExecuteProcessStep_WithValidData_CallsExpected()
    {
        // Arrange InitializeProcess
        var validProcessId = Guid.NewGuid();
        var credentialId = Guid.NewGuid();
        A.CallTo(() => _credentialRepository.GetDataForProcessId(validProcessId))
            .Returns((true, credentialId));

        // Act InitializeProcess
        var initializeResult = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        A.CallTo(() => _credentialCreationProcessHandler.CreateSignedCredential(credentialId, A<CancellationToken>._))
            .Returns((null, ProcessStepStatusId.DONE, false, null));

        // Act
        var result = await _sut.ExecuteProcessStep(ProcessStepTypeId.CREATE_SIGNED_CREDENTIAL, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ProcessMessage.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteProcessStep_WithRecoverableServiceException_ReturnsToDo()
    {
        // Arrange InitializeProcess
        var validProcessId = Guid.NewGuid();
        var credentialId = Guid.NewGuid();
        A.CallTo(() => _credentialRepository.GetDataForProcessId(validProcessId))
            .Returns((true, credentialId));

        // Act InitializeProcess
        var initializeResult = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        A.CallTo(() => _credentialCreationProcessHandler.CreateSignedCredential(credentialId, A<CancellationToken>._))
            .Throws(new ServiceException("this is a test", true));

        // Act
        var result = await _sut.ExecuteProcessStep(ProcessStepTypeId.CREATE_SIGNED_CREDENTIAL, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        result.Modified.Should().BeTrue();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO);
        result.ProcessMessage.Should().Be("this is a test");
        result.SkipStepTypeIds.Should().BeNull();
    }

    [Theory]
    [InlineData(ProcessStepTypeId.CREATE_SIGNED_CREDENTIAL, ProcessStepTypeId.RETRIGGER_CREATE_SIGNED_CREDENTIAL)]
    [InlineData(ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT, ProcessStepTypeId.RETRIGGER_SAVE_CREDENTIAL_DOCUMENT)]
    [InlineData(ProcessStepTypeId.REQUEST_CREDENTIAL_FOR_HOLDER, ProcessStepTypeId.RETRIGGER_REQUEST_CREDENTIAL_FOR_HOLDER)]
    [InlineData(ProcessStepTypeId.REQUEST_CREDENTIAL_AUTO_APPROVE, ProcessStepTypeId.RETRIGGER_REQUEST_CREDENTIAL_AUTO_APPROVE)]
    [InlineData(ProcessStepTypeId.REQUEST_CREDENTIAL_STATUS_CHECK, ProcessStepTypeId.RETRIGGER_REQUEST_CREDENTIAL_STATUS_CHECK)]
    [InlineData(ProcessStepTypeId.TRIGGER_CALLBACK, ProcessStepTypeId.RETRIGGER_TRIGGER_CALLBACK)]
    public async Task ExecuteProcessStep_WithServiceException_ReturnsFailedAndRetriggerStep(ProcessStepTypeId processStepTypeId, ProcessStepTypeId expectedRetriggerStep)
    {
        // Arrange InitializeProcess
        var validProcessId = Guid.NewGuid();
        var credentialId = Guid.NewGuid();
        A.CallTo(() => _credentialRepository.GetDataForProcessId(validProcessId))
            .Returns((true, credentialId));

        // Act InitializeProcess
        var initializeResult = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        A.CallTo(() => _credentialCreationProcessHandler.CreateSignedCredential(credentialId, A<CancellationToken>._))
            .Throws(new ServiceException("this is a test"));
        A.CallTo(() => _credentialCreationProcessHandler.SaveCredentialDocument(credentialId, A<CancellationToken>._))
            .Throws(new ServiceException("this is a test"));
        A.CallTo(() => _credentialCreationProcessHandler.RequestCredentialForHolder(credentialId, A<CancellationToken>._))
            .Throws(new ServiceException("this is a test"));
        A.CallTo(() => _credentialCreationProcessHandler.RequestCredentialAutoApprove(credentialId, A<CancellationToken>._))
            .Throws(new ServiceException("this is a test"));
        A.CallTo(() => _credentialCreationProcessHandler.CheckCredentialStatus(credentialId, A<CancellationToken>._))
            .Throws(new ServiceException("this is a test"));
        A.CallTo(() => _credentialCreationProcessHandler.TriggerCallback(credentialId, A<CancellationToken>._))
            .Throws(new ServiceException("this is a test"));

        // Act
        var result = await _sut.ExecuteProcessStep(processStepTypeId, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        result.Modified.Should().BeTrue();
        result.ScheduleStepTypeIds.Should().ContainSingle().Which.Should().Be(expectedRetriggerStep);
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        result.ProcessMessage.Should().Be("this is a test");
        result.SkipStepTypeIds.Should().BeNull();
    }

    #endregion
}
