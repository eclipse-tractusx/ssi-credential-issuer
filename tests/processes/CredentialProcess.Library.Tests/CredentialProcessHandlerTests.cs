using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Models;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.Tests;

public class CredentialProcessHandlerTests
{
    private readonly Guid _credentialId = Guid.NewGuid();

    private readonly IWalletBusinessLogic _walletBusinessLogic;
    private readonly IIssuerRepositories _issuerRepositories;
    private readonly ICredentialRepository _credentialRepository;

    private readonly CredentialProcessHandler _sut;
    private readonly IFixture _fixture;

    public CredentialProcessHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _issuerRepositories = A.Fake<IIssuerRepositories>();
        _credentialRepository = A.Fake<ICredentialRepository>();

        A.CallTo(() => _issuerRepositories.GetInstance<ICredentialRepository>()).Returns(_credentialRepository);

        _walletBusinessLogic = A.Fake<IWalletBusinessLogic>();

        _sut = new CredentialProcessHandler(_issuerRepositories, _walletBusinessLogic);
    }

    #region CreateCredential

    [Fact]
    public async Task CreateCredential_WithValidData_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetCredentialStorageInformationById(_credentialId))
            .Returns(new ValueTuple<VerifiedCredentialTypeKindId, JsonDocument>());

        // Act
        var result = await _sut.CreateCredential(_credentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _walletBusinessLogic.CreateCredential(_credentialId, A<JsonDocument>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.SIGN_CREDENTIAL);
    }

    #endregion

    #region SignCredential

    [Fact]
    public async Task SignCredential_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetWalletCredentialId(_credentialId))
            .Returns((Guid?)null);
        async Task Act() => await _sut.SignCredential(_credentialId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("ExternalCredentialId must be set here");
    }

    [Fact]
    public async Task SignCredential_WithValidData_ReturnsExpected()
    {
        // Arrange
        var externalCredentialId = Guid.NewGuid();
        A.CallTo(() => _credentialRepository.GetWalletCredentialId(_credentialId))
            .Returns(externalCredentialId);

        // Act
        var result = await _sut.SignCredential(_credentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _walletBusinessLogic.SignCredential(_credentialId, externalCredentialId, A<CancellationToken>._))
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
            .Returns(new ValueTuple<Guid?, VerifiedCredentialTypeKindId>());
        async Task Act() => await _sut.SaveCredentialDocument(_credentialId, CancellationToken.None).ConfigureAwait(false);

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
            .Returns(new ValueTuple<Guid?, VerifiedCredentialTypeKindId>(externalCredentialId, VerifiedCredentialTypeKindId.BPN));

        // Act
        var result = await _sut.SaveCredentialDocument(_credentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _walletBusinessLogic.GetCredential(_credentialId, externalCredentialId, VerifiedCredentialTypeKindId.BPN, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.CREATE_CREDENTIAL_FOR_HOLDER);
    }

    #endregion

    #region CreateCredentialForHolder

    [Fact]
    public async Task CreateCredentialForHolder_WithCredentialNotSet_SkipsStep()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetCredentialData(_credentialId))
            .Returns(new ValueTuple<HolderWalletData, string?, EncryptionTransformationData>());
        async Task Act() => await _sut.CreateCredentialForHolder(_credentialId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Credential must be set here");
    }

    [Fact]
    public async Task CreateCredentialForHolder_WithClientIdNull_SkipsStep()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetCredentialData(_credentialId))
            .Returns(new ValueTuple<HolderWalletData, string?, EncryptionTransformationData>(new HolderWalletData(null, null), "test", _fixture.Create<EncryptionTransformationData>()));

        // Act
        var result = await _sut.CreateCredentialForHolder(_credentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        result.nextStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task CreateCredentialForHolder_WithWalletUrlNull_SkipsStep()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetCredentialData(_credentialId))
            .Returns(new ValueTuple<HolderWalletData, string?, EncryptionTransformationData>(new HolderWalletData(null, "c1"), "test", _fixture.Create<EncryptionTransformationData>()));

        // Act
        var result = await _sut.CreateCredentialForHolder(_credentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        result.nextStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task CreateCredentialForHolder_WithEncryptionNotSet_SkipsStep()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetCredentialData(_credentialId))
            .Returns(new ValueTuple<HolderWalletData, string?, EncryptionTransformationData>(
                new HolderWalletData("https://example.org", "c1"),
                "test",
                new EncryptionTransformationData("test"u8.ToArray(), "test"u8.ToArray(), null)));

        // Act
        var result = await _sut.CreateCredentialForHolder(_credentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.SKIPPED);
        result.nextStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task CreateCredentialForHolder_WithValidData_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetCredentialData(_credentialId))
            .Returns(new ValueTuple<HolderWalletData, string?, EncryptionTransformationData>(
                new HolderWalletData("https://example.org", "c1"),
                "test",
                _fixture.Create<EncryptionTransformationData>()));

        // Act
        var result = await _sut.CreateCredentialForHolder(_credentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _walletBusinessLogic.CreateCredentialForHolder(_credentialId, "https://example.org", "c1", A<EncryptionInformation>._, "test", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().BeNull();
    }

    #endregion
}
