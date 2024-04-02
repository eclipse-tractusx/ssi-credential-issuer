using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Services;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Tests.BusinessLogic;

public class RevocationBusinessLogicTests
{
    private static readonly Guid CredentialId = Guid.NewGuid();
    private readonly IFixture _fixture;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICredentialRepository _credentialRepository;

    private readonly IRevocationBusinessLogic _sut;
    private readonly IIssuerRepositories _issuerRepositories;
    private readonly IWalletService _walletService;

    public RevocationBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _issuerRepositories = A.Fake<IIssuerRepositories>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _credentialRepository = A.Fake<ICredentialRepository>();
        _walletService = A.Fake<IWalletService>();

        A.CallTo(() => _issuerRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
        A.CallTo(() => _issuerRepositories.GetInstance<ICredentialRepository>()).Returns(_credentialRepository);

        _sut = new RevocationBusinessLogic(_issuerRepositories, _walletService);
    }

    #region RevokeIssuerCredential

    [Fact]
    public async Task RevokeIssuerCredential_WithNotExisting_ThrowsNotFoundException()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetRevocationDataById(CredentialId))
            .Returns(new ValueTuple<bool, Guid?, CompanySsiDetailStatusId, IEnumerable<ValueTuple<Guid, DocumentStatusId>>>());
        async Task Act() => await _sut.RevokeIssuerCredential(CredentialId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be(RevocationDataErrors.CREDENTIAL_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task RevokeIssuerCredential_WithExternalCredentialIdNotSet_ThrowsConflictException()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetRevocationDataById(CredentialId))
            .Returns(new ValueTuple<bool, Guid?, CompanySsiDetailStatusId, IEnumerable<ValueTuple<Guid, DocumentStatusId>>>(true, null, default, null!));
        async Task Act() => await _sut.RevokeIssuerCredential(CredentialId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be(RevocationDataErrors.EXTERNAL_CREDENTIAL_ID_NOT_SET.ToString());
    }

    [Theory]
    [InlineData(CompanySsiDetailStatusId.PENDING)]
    [InlineData(CompanySsiDetailStatusId.REVOKED)]
    [InlineData(CompanySsiDetailStatusId.INACTIVE)]
    public async Task RevokeIssuerCredential_WithStatusNotActiveRevoked_DoesNothing(CompanySsiDetailStatusId statusId)
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetRevocationDataById(CredentialId))
            .Returns(new ValueTuple<bool, Guid?, CompanySsiDetailStatusId, IEnumerable<ValueTuple<Guid, DocumentStatusId>>>(true, Guid.NewGuid(), statusId, null!));

        // Act
        await _sut.RevokeIssuerCredential(CredentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _walletService.RevokeCredentialForIssuer(A<Guid>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task RevokeIssuerCredential_WithValid_CallsExpected()
    {
        // Arrange
        var credential = new CompanySsiDetail(CredentialId, "Test", VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER, CompanySsiDetailStatusId.ACTIVE, "Test123", Guid.NewGuid().ToString(), DateTimeOffset.UtcNow)
        {
            ExternalCredentialId = Guid.NewGuid()
        };
        var document = _fixture
            .Build<Document>()
            .With(x => x.DocumentStatusId, DocumentStatusId.ACTIVE)
            .Create();
        A.CallTo(() => _credentialRepository.GetRevocationDataById(CredentialId))
            .Returns(new ValueTuple<bool, Guid?, CompanySsiDetailStatusId, IEnumerable<ValueTuple<Guid, DocumentStatusId>>>(true, credential.ExternalCredentialId, CompanySsiDetailStatusId.ACTIVE, Enumerable.Repeat(new ValueTuple<Guid, DocumentStatusId>(document.Id, document.DocumentStatusId), 1)));
        A.CallTo(() => _documentRepository.AttachAndModifyDocuments(A<IEnumerable<(Guid DocumentId, Action<Document>? Initialize, Action<Document> Modify)>>._))
            .Invokes((IEnumerable<(Guid DocumentId, Action<Document>? Initialize, Action<Document> Modify)> data) =>
            {
                data.Select(x =>
                    {
                        x.Initialize?.Invoke(document);
                        return document;
                    }
                ).ToImmutableArray();
                data.Select(x =>
                    {
                        x.Modify(document);
                        return document;
                    }
                ).ToImmutableArray();
            });
        A.CallTo(() => _credentialRepository.AttachAndModifyCredential(credential.Id, A<Action<CompanySsiDetail>>._, A<Action<CompanySsiDetail>>._))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> modify) =>
            {
                initialize?.Invoke(credential);
                modify(credential);
            });

        // Act
        await _sut.RevokeIssuerCredential(CredentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _walletService.RevokeCredentialForIssuer(A<Guid>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        document.DocumentStatusId.Should().Be(DocumentStatusId.INACTIVE);
        credential.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.REVOKED);
    }

    #endregion

    #region RevokeHolderCredential

    [Fact]
    public async Task RevokeHolderCredential_WithNotExisting_ThrowsNotFoundException()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetRevocationDataById(CredentialId))
            .Returns(new ValueTuple<bool, Guid?, CompanySsiDetailStatusId, IEnumerable<ValueTuple<Guid, DocumentStatusId>>>());
        async Task Act() => await _sut.RevokeHolderCredential(CredentialId, _fixture.Create<TechnicalUserDetails>(), CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be(RevocationDataErrors.CREDENTIAL_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task RevokeHolderCredential_WithExternalCredentialIdNotSet_ThrowsConflictException()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetRevocationDataById(CredentialId))
            .Returns(new ValueTuple<bool, Guid?, CompanySsiDetailStatusId, IEnumerable<ValueTuple<Guid, DocumentStatusId>>>(true, null, default, null!));
        async Task Act() => await _sut.RevokeHolderCredential(CredentialId, _fixture.Create<TechnicalUserDetails>(), CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be(RevocationDataErrors.EXTERNAL_CREDENTIAL_ID_NOT_SET.ToString());
    }

    [Theory]
    [InlineData(CompanySsiDetailStatusId.PENDING)]
    [InlineData(CompanySsiDetailStatusId.REVOKED)]
    [InlineData(CompanySsiDetailStatusId.INACTIVE)]
    public async Task RevokeHolderCredential_WithStatusNotActiveRevoked_DoesNothing(CompanySsiDetailStatusId statusId)
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetRevocationDataById(CredentialId))
            .Returns(new ValueTuple<bool, Guid?, CompanySsiDetailStatusId, IEnumerable<ValueTuple<Guid, DocumentStatusId>>>(true, Guid.NewGuid(), statusId, null!));

        // Act
        await _sut.RevokeHolderCredential(CredentialId, _fixture.Create<TechnicalUserDetails>(), CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _walletService.RevokeCredentialForHolder(A<string>._, A<string>._, A<string>._, A<Guid>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task RevokeHolderCredential_WithValid_CallsExpected()
    {
        // Arrange
        var credential = new CompanySsiDetail(CredentialId, "Test", VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER, CompanySsiDetailStatusId.ACTIVE, "Test123", Guid.NewGuid().ToString(), DateTimeOffset.UtcNow)
        {
            ExternalCredentialId = Guid.NewGuid()
        };
        var document = _fixture
            .Build<Document>()
            .With(x => x.DocumentStatusId, DocumentStatusId.ACTIVE)
            .Create();
        A.CallTo(() => _credentialRepository.GetRevocationDataById(CredentialId))
            .Returns(new ValueTuple<bool, Guid?, CompanySsiDetailStatusId, IEnumerable<ValueTuple<Guid, DocumentStatusId>>>(true, credential.ExternalCredentialId, CompanySsiDetailStatusId.ACTIVE, Enumerable.Repeat(new ValueTuple<Guid, DocumentStatusId>(document.Id, document.DocumentStatusId), 1)));
        A.CallTo(() => _documentRepository.AttachAndModifyDocuments(A<IEnumerable<(Guid DocumentId, Action<Document>? Initialize, Action<Document> Modify)>>._))
            .Invokes((IEnumerable<(Guid DocumentId, Action<Document>? Initialize, Action<Document> Modify)> data) =>
            {
                data.Select(x =>
                    {
                        x.Initialize?.Invoke(document);
                        return document;
                    }
                ).ToImmutableArray();
                data.Select(x =>
                    {
                        x.Modify(document);
                        return document;
                    }
                ).ToImmutableArray();
            });
        A.CallTo(() => _credentialRepository.AttachAndModifyCredential(credential.Id, A<Action<CompanySsiDetail>>._, A<Action<CompanySsiDetail>>._))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> modify) =>
            {
                initialize?.Invoke(credential);
                modify(credential);
            });

        // Act
        await _sut.RevokeHolderCredential(CredentialId, _fixture.Create<TechnicalUserDetails>(), CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _walletService.RevokeCredentialForHolder(A<string>._, A<string>._, A<string>._, A<Guid>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        document.DocumentStatusId.Should().Be(DocumentStatusId.INACTIVE);
        credential.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.REVOKED);
    }

    #endregion
}
