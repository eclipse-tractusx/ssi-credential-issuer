using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Services;
using System.Security.Cryptography;
using System.Text.Json;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Tests.BusinessLogic;

public class WalletBusinessLogicTests
{
    private static readonly string IssuerBpnl = "BPNL000001ISSUER";

    private readonly WalletBusinessLogic _sut;
    private readonly IWalletService _walletService;
    private readonly ICompanySsiDetailsRepository _companySsiDetailRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly EncryptionModeConfig _encryptionModeConfig;

    public WalletBusinessLogicTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _encryptionModeConfig = new EncryptionModeConfig
        {
            Index = 0,
            CipherMode = CipherMode.ECB,
            PaddingMode = PaddingMode.PKCS7,
            EncryptionKey = "202048656c6c6f20202048656c6c6f20202048656c6c6f20202048656c6c6f20"
        };
        var options = Options.Create(new WalletSettings
        {
            BaseAddress = "https://base.address.com",
            ClientId = "CatenaX",
            ClientSecret = "pass@Secret",
            TokenAddress = "https://example.org/token",
            EncryptionConfigs = Enumerable.Repeat(_encryptionModeConfig, 1),
            EncrptionConfigIndex = 0
        });
        _walletService = A.Fake<IWalletService>();
        var issuerRepositories = A.Fake<IIssuerRepositories>();
        _companySsiDetailRepository = A.Fake<ICompanySsiDetailsRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        A.CallTo(() => issuerRepositories.GetInstance<ICompanySsiDetailsRepository>())
            .Returns(_companySsiDetailRepository);
        A.CallTo(() => issuerRepositories.GetInstance<IDocumentRepository>())
            .Returns(_documentRepository);

        _sut = new WalletBusinessLogic(_walletService, issuerRepositories, options);
    }

    #region CreateCredential

    [Fact]
    public async Task CreateCredential_CallsExpected()
    {
        // Arrange
        var id = Guid.NewGuid();
        var externalId = Guid.NewGuid();
        var schema = JsonDocument.Parse("{}");
        var ssiDetail = new CompanySsiDetail(id, null!, VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER, CompanySsiDetailStatusId.ACTIVE, IssuerBpnl, Guid.NewGuid().ToString(), DateTimeOffset.UtcNow);
        A.CallTo(() => _companySsiDetailRepository.AttachAndModifyCompanySsiDetails(A<Guid>._, A<Action<CompanySsiDetail>>._, A<Action<CompanySsiDetail>>._))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> setupOptionalFields) =>
            {
                initialize?.Invoke(ssiDetail);
                setupOptionalFields(ssiDetail);
            });
        A.CallTo(() => _walletService.CreateCredential(schema, A<CancellationToken>._))
            .Returns(externalId);

        // Act
        await _sut.CreateCredential(id, schema, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companySsiDetailRepository.AttachAndModifyCompanySsiDetails(id, A<Action<CompanySsiDetail>>._, A<Action<CompanySsiDetail>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _walletService.CreateCredential(schema, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        ssiDetail.ExternalCredentialId = externalId;
    }

    #endregion

    #region SignCredential

    [Fact]
    public async Task SignCredential_CallsExpected()
    {
        // Arrange
        var id = Guid.NewGuid();
        var credentialId = Guid.NewGuid();
        var ssiDetail = new CompanySsiDetail(id, null!, VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER, CompanySsiDetailStatusId.ACTIVE, IssuerBpnl, Guid.NewGuid().ToString(), DateTimeOffset.UtcNow);
        A.CallTo(() => _companySsiDetailRepository.AttachAndModifyCompanySsiDetails(A<Guid>._, A<Action<CompanySsiDetail>>._, A<Action<CompanySsiDetail>>._))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> setupOptionalFields) =>
            {
                initialize?.Invoke(ssiDetail);
                setupOptionalFields(ssiDetail);
            });
        A.CallTo(() => _walletService.SignCredential(credentialId, A<CancellationToken>._))
            .Returns("cred");

        // Act
        await _sut.SignCredential(id, credentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companySsiDetailRepository.AttachAndModifyCompanySsiDetails(id, A<Action<CompanySsiDetail>>._, A<Action<CompanySsiDetail>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _walletService.SignCredential(credentialId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        ssiDetail.Credential.Should().Be("cred");
    }

    #endregion

    #region CreateCredentialForHolder

    [Fact]
    public async Task CreateCredentialForHolder_CallsExpected()
    {
        // Arrange
        var id = Guid.NewGuid();
        var processData = new CompanySsiProcessData(id, null!, VerifiedCredentialTypeKindId.BPN) { ClientId = "123" };
        var (secret, vector) = CryptoHelper.Encrypt("test", Convert.FromHexString(_encryptionModeConfig.EncryptionKey), _encryptionModeConfig.CipherMode, _encryptionModeConfig.PaddingMode);
        A.CallTo(() => _companySsiDetailRepository.AttachAndModifyProcessData(A<Guid>._, A<Action<CompanySsiProcessData>>._, A<Action<CompanySsiProcessData>>._))
            .Invokes((Guid _, Action<CompanySsiProcessData>? initialize, Action<CompanySsiProcessData> setupOptionalFields) =>
            {
                initialize?.Invoke(processData);
                setupOptionalFields(processData);
            });

        // Act
        await _sut.CreateCredentialForHolder(id, "https://example.org/wallet", "test1", new EncryptionInformation(secret, vector, 0), "thisisatestsecret", CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _companySsiDetailRepository.AttachAndModifyProcessData(id, A<Action<CompanySsiProcessData>>._, A<Action<CompanySsiProcessData>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _walletService.CreateCredentialForHolder(A<string>._, A<string>._, A<string>._, A<string>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        processData.ClientId.Should().BeNull();
        processData.ClientSecret.Should().BeNull();
    }

    #endregion

    #region GetCredential

    [Fact]
    public async Task GetCredential_CallsExpected()
    {
        // Arrange
        const string data = """
            {
                "id": "f2ee21e938e217f362d3e7edac9e59a2b17d34017ef0f842e1f48513",
                "type": [
                    "VerifiableCredential",
                    "BpnCredential"
                ],
                "issuer": "did:web:example.org:dim-hosted:2f45795c-d6cc-4038-96c9-63cedc0cd266:holder-iatp",
                "@context": [
                    "https://www.w3.org/2018/credentials/v1",
                    "https://w3id.org/catenax/credentials/v1.0.0"
                ],
                "issuanceDate": "2024-04-22T17:02:33.568629+00:00",
                "expirationDate": "2025-04-22T17:02:33.568629+00:00",
                "credentialStatus": {
                    "id": "https://example.org/credentials/status/c5f1d9bb-42d5-42b7-a80a-ccbda6891df3/3e6f1f74-56e3-443a-a75e-320c301aca07#10",
                    "type": "StatusList2021Entry",
                    "statusPurpose": "revocation",
                    "statusListIndex": "10",
                    "statusListCredential": "https://example.org/credentials/status/c5f1d9bb-42d5-42b7-a80a-ccbda6891df3/3e6f1f74-56e3-443a-a75e-320c301aca07"
                },
                "credentialSubject": {
                    "id": "did:web:example.org:api:administration:staticdata:did:BPNL000001PS0000",
                    "bpn": "BPNL000001PS0000",
                    "holderIndentifier": "BPNL000001PS0000"
                }
            }
            """;
        var id = Guid.NewGuid();
        var credentialId = Guid.NewGuid();
        var jsonDocument = JsonDocument.Parse(data);
        A.CallTo(() => _walletService.GetCredential(credentialId, A<CancellationToken>._))
            .Returns(jsonDocument);

        // Act
        await _sut.GetCredential(id, credentialId, VerifiedCredentialTypeKindId.BPN, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, MediaTypeId.JSON, DocumentTypeId.VERIFIED_CREDENTIAL, A<Action<Document>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.AssignDocumentToCompanySsiDetails(A<Guid>._, id))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetCredential_WithSchemaNotMatching_CallsExpected()
    {
        // Arrange
        const string data = """
                            {
                                "id": "2e70ee49-5fae-438a-9435-0cce3854650d",
                                "@context": [
                                    "https://www.w3.org/2018/credentials/v1",
                                    "https://w3id.org/catenax/credentials/v1.0.0"
                                ],
                                "expirationDate": "2022-06-16T18:56:59Z",
                                "credentialSubject": {
                                    "id": "2e70ee49-5fae-438a-9435-0cce3854650d",
                                    "holderIdentifier": "2e70ee49-5fae-438a-9435-0cce3854650d",
                                    "bpn": "2e70ee49-5fae-438a-9435-0cce3854650d"
                                }
                            }
                            """;
        var id = Guid.NewGuid();
        var credentialId = Guid.NewGuid();
        var jsonDocument = JsonDocument.Parse(data);
        A.CallTo(() => _walletService.GetCredential(credentialId, A<CancellationToken>._))
            .Returns(jsonDocument);
        async Task Act() => await _sut.GetCredential(id, credentialId, VerifiedCredentialTypeKindId.BPN, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ServiceException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be("Invalid schema for type BPN");
    }

    #endregion
}
