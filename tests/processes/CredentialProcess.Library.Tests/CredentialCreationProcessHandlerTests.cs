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
using Org.Eclipse.TractusX.SsiCredentialIssuer.Callback.Service.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Callback.Service.Services;
using Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.Creation;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Models;
using System.Text.Json;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.Tests;

public class CredentialCreationProcessHandlerTests
{
    private readonly Guid _credentialId = Guid.NewGuid();

    private readonly IWalletBusinessLogic _walletBusinessLogic;
    private readonly ICredentialRepository _credentialRepository;

    private readonly CredentialCreationProcessHandler _sut;
    private readonly IFixture _fixture;
    private readonly ICallbackService _callbackService;

    public CredentialCreationProcessHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var issuerRepositories = A.Fake<IIssuerRepositories>();
        _credentialRepository = A.Fake<ICredentialRepository>();

        A.CallTo(() => issuerRepositories.GetInstance<ICredentialRepository>()).Returns(_credentialRepository);

        _walletBusinessLogic = A.Fake<IWalletBusinessLogic>();
        _callbackService = A.Fake<ICallbackService>();

        _sut = new CredentialCreationProcessHandler(issuerRepositories, _walletBusinessLogic, _callbackService);
    }

    #region CreateCredential

    [Fact]
    public async Task CreateCredential_WithValidData_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetCredentialStorageInformationById(_credentialId))
            .Returns(default((VerifiedCredentialTypeKindId, JsonDocument)));

        // Act
        var result = await _sut.CreateSignedCredential(_credentialId, CancellationToken.None);

        // Assert
        A.CallTo(() => _walletBusinessLogic.CreateSignedCredential(_credentialId, A<JsonDocument>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT);
    }

    #endregion

    #region SaveCredentialDocument

    [Fact]
    public async Task SaveCredentialDocument_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetExternalCredentialAndKindId(_credentialId))
            .Returns(default((Guid?, VerifiedCredentialTypeKindId, bool, string?)));
        Task Act() => _sut.SaveCredentialDocument(_credentialId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("ExternalCredentialId must be set here");
    }

    [Fact]
    public async Task SaveCredentialDocument_WithValidData_ReturnsExpected()
    {
        // Arrange
        var externalCredentialId = Guid.NewGuid();
        A.CallTo(() => _credentialRepository.GetExternalCredentialAndKindId(_credentialId))
            .Returns((externalCredentialId, VerifiedCredentialTypeKindId.BPN, true, "https://example.org"));

        // Act
        var result = await _sut.SaveCredentialDocument(_credentialId, CancellationToken.None);

        // Assert
        A.CallTo(() => _walletBusinessLogic.GetCredential(_credentialId, externalCredentialId, VerifiedCredentialTypeKindId.BPN, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.REQUEST_CREDENTIAL_FOR_HOLDER);
    }

    #endregion

    #region RequestCredentialForHolder

    [Fact]
    public async Task RequestCredentialForHolder_WithCredentialNotSet_SkipsStep()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetCredentialData(_credentialId))
            .Returns(default((bool, HolderWalletData, string?, JsonDocument?, EncryptionTransformationData, string?)));
        Task Act() => _sut.RequestCredentialForHolder(_credentialId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Credential must be set here");
    }

    [Fact]
    public async Task RequestCredentialForHolder_WithClientIdNull_SkipsStep()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetCredentialData(_credentialId))
            .Returns((false, new HolderWalletData(null, null), "test", JsonDocument.Parse(@"{""name"":""John"",""age"":30}"), _fixture.Create<EncryptionTransformationData>(), "https://example.org"));

        // Act
        var result = await _sut.RequestCredentialForHolder(_credentialId, CancellationToken.None);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().Be("ProcessStep was skipped because the holder is the BYOW");
        result.stepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.TRIGGER_CALLBACK);
    }

    [Fact]
    public async Task RequestCredentialForHolder_WithWalletUrlNull_SkipsStep()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetCredentialData(_credentialId))
            .Returns((false, new HolderWalletData(null, "c1"), "test", JsonDocument.Parse(@"{""name"":""John"",""age"":30}"), _fixture.Create<EncryptionTransformationData>(), "https://example.org"));

        // Act
        var result = await _sut.RequestCredentialForHolder(_credentialId, CancellationToken.None);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().Be("ProcessStep was skipped because the holder is the BYOW");
        result.stepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.TRIGGER_CALLBACK);
    }

    [Fact]
    public async Task RequestCredentialForHolder_WithEncryptionNotSet_SkipsStep()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetCredentialData(_credentialId))
            .Returns((
                false,
                new HolderWalletData("https://example.org", "c1"),
                "test",
                JsonDocument.Parse(@"{""name"":""John"",""age"":30}"),
                new EncryptionTransformationData("test"u8.ToArray(), "test"u8.ToArray(), 0),
                "https://example.org"));

        // Act
        var result = await _sut.RequestCredentialForHolder(_credentialId, CancellationToken.None);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.REQUEST_CREDENTIAL_STATUS_CHECK);
    }

    [Fact]
    public async Task RequestCredentialForHolder_WithValidData_ReturnsExpected()
    {
        // Arrange
        var credJson = JsonDocument.Parse(@"{""name"":""John"",""age"":30}");
        A.CallTo(() => _credentialRepository.GetCredentialData(_credentialId))
            .Returns((
                false,
                new HolderWalletData("https://example.org", "c1"),
                "test",
                credJson,
                _fixture.Create<EncryptionTransformationData>(),
                "https://example.org"));

        // Act
        var result = await _sut.RequestCredentialForHolder(_credentialId, CancellationToken.None);

        // Assert
        A.CallTo(() => _walletBusinessLogic.RequestCredentialForHolder(_credentialId, "https://example.org", "c1", A<EncryptionInformation>._, credJson.RootElement.GetRawText(), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.REQUEST_CREDENTIAL_STATUS_CHECK);
    }

    [Fact]
    public async Task RequestCredentialForHolder_WithIssuerAsHolder_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetCredentialData(_credentialId))
            .Returns((
                true,
                new HolderWalletData("https://example.org", "c1"),
                "test",
                JsonDocument.Parse(@"{""name"":""John"",""age"":30}"),
                _fixture.Create<EncryptionTransformationData>(),
                "https://example.org"));

        // Act
        var result = await _sut.RequestCredentialForHolder(_credentialId, CancellationToken.None);

        // Assert
        A.CallTo(() => _walletBusinessLogic.RequestCredentialForHolder(A<Guid>._, A<string>._, A<string>._, A<EncryptionInformation>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        result.modified.Should().BeFalse();
        result.processMessage.Should().Be("ProcessStep was skipped because the holder is the issuer");
        result.stepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.TRIGGER_CALLBACK);
    }

    #endregion

    #region TriggerCallback

    [Fact]
    public async Task TriggerCallback_WithCallbackUrlNotSet_ThrowsConflictException()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetCallbackUrl(_credentialId))
            .Returns(("BPNL000001234", null));
        Task Act() => _sut.TriggerCallback(_credentialId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("CallbackUrl must be set");
    }

    [Fact]
    public async Task TriggerCallback_WithValid_CallsExpected()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetCallbackUrl(_credentialId))
            .Returns(("BPNL00000123456", "https://example.org"));

        // Act
        var result = await _sut.TriggerCallback(_credentialId, CancellationToken.None);

        // Assert
        A.CallTo(() => _callbackService.TriggerCallback("https://example.org", A<IssuerResponseData>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().BeNull();
    }

    #endregion
}
