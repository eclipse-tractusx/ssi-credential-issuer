using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Services;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Tests.BusinessLogic;

public class RevocationBusinessLogicTests
{
    private static readonly Guid CredentialId = Guid.NewGuid();
    private static readonly string Bpnl = "BPNL00000001TEST";
    private readonly IFixture _fixture;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICredentialRepository _credentialRepository;

    private readonly IRevocationBusinessLogic _sut;
    private readonly IWalletService _walletService;
    private readonly IIdentityService _identityService;
    private readonly IIdentityData _identityData;
    private readonly IIssuerRepositories _issuerRepositories;

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
        _identityService = A.Fake<IIdentityService>();
        _identityData = A.Fake<IIdentityData>();
        A.CallTo(() => _identityData.Bpnl).Returns(Bpnl);
        A.CallTo(() => _identityService.IdentityData).Returns(_identityData);

        A.CallTo(() => _issuerRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
        A.CallTo(() => _issuerRepositories.GetInstance<ICredentialRepository>()).Returns(_credentialRepository);

        _sut = new RevocationBusinessLogic(_issuerRepositories, _walletService, _identityService);
    }

    #region RevokeIssuerCredential

    [Fact]
    public async Task RevokeIssuerCredential_WithNotExisting_ThrowsNotFoundException()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetRevocationDataById(CredentialId, Bpnl))
            .Returns(default((bool, bool, Guid?, CompanySsiDetailStatusId, IEnumerable<(Guid, DocumentStatusId)>)));
        Task Act() => _sut.RevokeCredential(CredentialId, true, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be(RevocationDataErrors.CREDENTIAL_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task RevokeIssuerCredential_WithNotAllowed_ThrowsForbiddenException()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetRevocationDataById(CredentialId, Bpnl))
            .Returns((true, false, null, default, null!));
        Task Act() => _sut.RevokeCredential(CredentialId, false, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);

        // Assert
        ex.Message.Should().Be(RevocationDataErrors.NOT_ALLOWED_TO_REVOKE_CREDENTIAL.ToString());
    }

    [Fact]
    public async Task RevokeIssuerCredential_WithExternalCredentialIdNotSet_ThrowsConflictException()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetRevocationDataById(CredentialId, Bpnl))
            .Returns((true, true, null, default, null!));
        Task Act() => _sut.RevokeCredential(CredentialId, true, CancellationToken.None);

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
        A.CallTo(() => _credentialRepository.GetRevocationDataById(CredentialId, Bpnl))
            .Returns((true, true, Guid.NewGuid(), statusId, null!));

        // Act
        await _sut.RevokeCredential(CredentialId, true, CancellationToken.None);

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
        A.CallTo(() => _credentialRepository.GetRevocationDataById(CredentialId, Bpnl))
            .Returns((true, true, credential.ExternalCredentialId, CompanySsiDetailStatusId.ACTIVE, Enumerable.Repeat((document.Id, document.DocumentStatusId), 1)));
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
        await _sut.RevokeCredential(CredentialId, true, CancellationToken.None);

        // Assert
        A.CallTo(() => _walletService.RevokeCredentialForIssuer(A<Guid>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        document.DocumentStatusId.Should().Be(DocumentStatusId.INACTIVE);
        credential.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.REVOKED);
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #endregion
}
