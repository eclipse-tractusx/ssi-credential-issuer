using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Json.More;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Tests.Shared;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Services;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Tests.Services;

public class WalletServiceTests
{
    private readonly WalletService _sut;
    private readonly IBasicAuthTokenService _basicAuthTokenService;
    private readonly IOptions<WalletSettings> _options;

    public WalletServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _basicAuthTokenService = A.Fake<IBasicAuthTokenService>();

        _options = Options.Create(new WalletSettings
        {
            BaseAddress = "https://base.address.com",
            ClientId = "CatenaX",
            ClientSecret = "pass@Secret",
            TokenAddress = "https://example.org/token",
            EncryptionConfigIndex = 0,
            WalletApplication = "catena-x-portal",
            CreateSignedCredentialPath = "/api/v2.0.0/credentials",
            CreateCredentialPath = "api/v2.0.0/credentials",
            GetCredentialPath = "/api/v2.0.0/credentials/{0}",
            RevokeCredentialPath = "/api/v2.0.0/credentials/{0}",
            RequestCredentialPath = "/api/v2.0.0/dcp/requestCredentials/{0}",
            CredentialRequestsReceivedAutoApprovePath = "/api/v2.0.0/dcp/credentialRequestsReceived/{0}/autoApprove",
            CredentialRequestsReceivedPath = "/api/v2.0.0/dcp/credentialRequestsReceived",
            CredentialRequestsReceivedDetailPath = "/api/v2.0.0/dcp/credentialRequestsReceived/{0}"
        });
        _sut = new WalletService(_basicAuthTokenService, _options);
    }

    #region CreateCredential

    [Fact]
    public async Task CreateCredential_WithValid_DoesNotThrowException()
    {
        // Arrange
        var payload = JsonDocument.Parse("{}");
        var id = Guid.NewGuid();
        var response = new CreateCredentialResponse(id);
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(response)));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        var result = await _sut.CreateSignedCredential(payload, CancellationToken.None);

        // Assert
        httpMessageHandlerMock.RequestMessage.Should().Match<HttpRequestMessage>(x =>
            x.Content is JsonContent &&
            (x.Content as JsonContent)!.ObjectType == typeof(CreateSignedCredentialRequest) &&
            ((x.Content as JsonContent)!.Value as CreateSignedCredentialRequest)!.Application == "catena-x-portal" &&
            ((x.Content as JsonContent)!.Value as CreateSignedCredentialRequest)!.Issue.Payload.Signature.ProofMechanism == "external" &&
            ((x.Content as JsonContent)!.Value as CreateSignedCredentialRequest)!.Issue.Payload.Signature.ProofType == "jwt" &&
            ((x.Content as JsonContent)!.Value as CreateSignedCredentialRequest)!.Issue.Payload.Signature.KeyName == null &&
            ((x.Content as JsonContent)!.Value as CreateSignedCredentialRequest)!.Issue.Payload.Content == payload
        );
        result.Should().BeOfType<CreateSignedCredentialResponse>().Which.Id.Should().Be(id);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "{ \"message\": \"Framework test!\" }", "call to external system create-credential failed with statuscode 409 - Message: { \"message\": \"Framework test!\" }")]
    [InlineData(HttpStatusCode.BadRequest, "{ \"test\": \"123\" }", "call to external system create-credential failed with statuscode 400 - Message: { \"test\": \"123\" }")]
    [InlineData(HttpStatusCode.BadRequest, "this is no json", "call to external system create-credential failed with statuscode 400 - Message: this is no json")]
    [InlineData(HttpStatusCode.Forbidden, null, "call to external system create-credential failed with statuscode 403")]
    public async Task CreateCredential_WithConflict_ThrowsServiceExceptionWithErrorContent(HttpStatusCode statusCode, string? content, string message)
    {
        // Arrange
        var payload = JsonDocument.Parse("{}");
        var httpMessageHandlerMock = content == null
            ? new HttpMessageHandlerMock(statusCode)
            : new HttpMessageHandlerMock(statusCode, new StringContent(content));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);

        // Act
        async Task Act() => await _sut.CreateSignedCredential(payload, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion

    #region GetCredential

    [Fact]
    public async Task GetCredential_WithValid_DoesNotThrowException()
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        var json = """
                   {
                        "root": "123"
                   }
                   """;
        var response = new GetCredentialResponse("test", JsonDocument.Parse(json), "test123", "VALID");
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(response)));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        var result = await _sut.GetCredential(credentialId, CancellationToken.None);

        // Assert
        result.RootElement.ToJsonString().Should().Be("{\"root\":\"123\"}");
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "{ \"message\": \"Framework test!\" }", "call to external system get-credential failed with statuscode 409 - Message: { \"message\": \"Framework test!\" }")]
    [InlineData(HttpStatusCode.BadRequest, "{ \"test\": \"123\" }", "call to external system get-credential failed with statuscode 400 - Message: { \"test\": \"123\" }")]
    [InlineData(HttpStatusCode.BadRequest, "this is no json", "call to external system get-credential failed with statuscode 400 - Message: this is no json")]
    [InlineData(HttpStatusCode.Forbidden, null, "call to external system get-credential failed with statuscode 403")]
    public async Task GetCredential_WithConflict_ThrowsServiceExceptionWithErrorContent(HttpStatusCode statusCode, string? content, string message)
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        var httpMessageHandlerMock = content == null
            ? new HttpMessageHandlerMock(statusCode)
            : new HttpMessageHandlerMock(statusCode, new StringContent(content));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);

        // Act
        async Task Act() => await _sut.GetCredential(credentialId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion

    #region CreateCredentialForHolder

    [Fact]
    public async Task CreateCredentialForHolder_WithValid_DoesNotThrowException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = new CreateCredentialResponse(id);
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(response)));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(A<BasicAuthSettings>._, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        var result = await _sut.CreateCredentialForHolder("https://example.org", "test", "testSec", "testCred", CancellationToken.None);

        // Assert
        httpMessageHandlerMock.RequestMessage.Should().Match<HttpRequestMessage>(x =>
            x.Content is JsonContent &&
            (x.Content as JsonContent)!.ObjectType == typeof(DeriveCredentialData) &&
            ((x.Content as JsonContent)!.Value as DeriveCredentialData)!.Application == "catena-x-portal"
        );
        result.Should().Be(id);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "{ \"message\": \"Framework test!\" }", "call to external system create-holder-credential failed with statuscode 409 - Message: { \"message\": \"Framework test!\" }")]
    [InlineData(HttpStatusCode.BadRequest, "{ \"test\": \"123\" }", "call to external system create-holder-credential failed with statuscode 400 - Message: { \"test\": \"123\" }")]
    [InlineData(HttpStatusCode.BadRequest, "this is no json", "call to external system create-holder-credential failed with statuscode 400 - Message: this is no json")]
    [InlineData(HttpStatusCode.Forbidden, null, "call to external system create-holder-credential failed with statuscode 403")]
    public async Task CreateCredentialForHolder_WithConflict_ThrowsServiceExceptionWithErrorContent(HttpStatusCode statusCode, string? content, string message)
    {
        // Arrange
        var httpMessageHandlerMock = content == null
            ? new HttpMessageHandlerMock(statusCode)
            : new HttpMessageHandlerMock(statusCode, new StringContent(content));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(A<BasicAuthSettings>._, A<CancellationToken>._)).Returns(httpClient);

        // Act
        async Task Act() => await _sut.CreateCredentialForHolder("https://example.org", "test", "testSec", "testCred", CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion

    #region RevokeCredentialForIssuer

    [Fact]
    public async Task RevokeCredentialForIssuer_WithValid_DoesNotThrowException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = new CreateCredentialResponse(id);
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(response)));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(A<BasicAuthSettings>._, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        await _sut.RevokeCredentialForIssuer(Guid.NewGuid(), CancellationToken.None);

        // Assert
        httpMessageHandlerMock.RequestMessage.Should().Match<HttpRequestMessage>(x =>
            x.Content is JsonContent &&
            (x.Content as JsonContent)!.ObjectType == typeof(RevokeCredentialRequest) &&
            ((x.Content as JsonContent)!.Value as RevokeCredentialRequest)!.Payload.Revoke);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "{ \"message\": \"Framework test!\" }", "call to external system revoke-credential failed with statuscode 409 - Message: { \"message\": \"Framework test!\" }")]
    [InlineData(HttpStatusCode.BadRequest, "{ \"test\": \"123\" }", "call to external system revoke-credential failed with statuscode 400 - Message: { \"test\": \"123\" }")]
    [InlineData(HttpStatusCode.BadRequest, "this is no json", "call to external system revoke-credential failed with statuscode 400 - Message: this is no json")]
    [InlineData(HttpStatusCode.Forbidden, null, "call to external system revoke-credential failed with statuscode 403")]
    public async Task RevokeCredentialForIssuer_WithConflict_ThrowsServiceExceptionWithErrorContent(HttpStatusCode statusCode, string? content, string message)
    {
        // Arrange
        var httpMessageHandlerMock = content == null
            ? new HttpMessageHandlerMock(statusCode)
            : new HttpMessageHandlerMock(statusCode, new StringContent(content));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(A<BasicAuthSettings>._, A<CancellationToken>._)).Returns(httpClient);

        // Act
        async Task Act() => await _sut.RevokeCredentialForIssuer(Guid.NewGuid(), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion

    #region RequestCredentialForHolder

    [Fact]
    public async Task RequestCredentialForHolder_WithValid_ReturnsGuid()
    {
        // Arrange
        var credential = """
                            {
                                "id": "2e70ee49-5fae-438a-9435-0cce3854650d",
                                "@context": [
                                    "https://www.w3.org/2018/credentials/v1",
                                    "https://w3id.org/catenax/credentials/v1.0.0"
                                ],
                                "type": [
                                    "VerifiableCredential",
                                    "BpnCredential"
                                ],
                                "issuer": "did:web:example.org:issuer",
                                "expirationDate": "2022-06-16T18:56:59Z",
                                "credentialSubject": {
                                    "id": "did:web:example.org:holder",
                                    "holderIdentifier": "2e70ee49-5fae-438a-9435-0cce3854650d",
                                    "bpn": "2e70ee49-5fae-438a-9435-0cce3854650d"
                                }
                            }
                            """;
        var id = Guid.NewGuid();
        var response = new RequestCredentialResponse(id);
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(response)));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(A<BasicAuthSettings>._, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        var result = await _sut.RequestCredentialForHolder("https://example.org", "test", "testSec", credential, CancellationToken.None);

        // Assert
        result.Should().Be(id);
        httpMessageHandlerMock.RequestMessage.Should().Match<HttpRequestMessage>(x =>
            x.Content is JsonContent &&
            (x.Content as JsonContent)!.ObjectType == typeof(RequestCredential));
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "{ \"message\": \"Framework test!\" }", "call to external system request-holder-credential failed with statuscode 409 - Message: { \"message\": \"Framework test!\" }")]
    [InlineData(HttpStatusCode.BadRequest, "{ \"test\": \"123\" }", "call to external system request-holder-credential failed with statuscode 400 - Message: { \"test\": \"123\" }")]
    [InlineData(HttpStatusCode.BadRequest, "this is no json", "call to external system request-holder-credential failed with statuscode 400 - Message: this is no json")]
    [InlineData(HttpStatusCode.Forbidden, null, "call to external system request-holder-credential failed with statuscode 403")]
    public async Task RequestCredentialForHolder_WithConflict_ThrowsServiceException(HttpStatusCode statusCode, string? content, string message)
    {
        // Arrange
        var credential = """
                            {
                                "id": "2e70ee49-5fae-438a-9435-0cce3854650d",
                                "@context": [
                                    "https://www.w3.org/2018/credentials/v1",
                                    "https://w3id.org/catenax/credentials/v1.0.0"
                                ],
                                "type": [
                                    "VerifiableCredential",
                                    "BpnCredential"
                                ],
                                "issuer": "did:web:example.org:issuer",
                                "expirationDate": "2022-06-16T18:56:59Z",
                                "credentialSubject": {
                                    "id": "did:web:example.org:holder",
                                    "holderIdentifier": "2e70ee49-5fae-438a-9435-0cce3854650d",
                                    "bpn": "2e70ee49-5fae-438a-9435-0cce3854650d"
                                }
                            }
                            """;
        var httpMessageHandlerMock = content == null
            ? new HttpMessageHandlerMock(statusCode)
            : new HttpMessageHandlerMock(statusCode, new StringContent(content));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(A<BasicAuthSettings>._, A<CancellationToken>._)).Returns(httpClient);

        // Act
        async Task Act() => await _sut.RequestCredentialForHolder("https://example.org", "test", "testSec", credential, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion

    #region GetCredentialRequestsReceived

    [Fact]
    public async Task GetCredentialRequestsReceived_WithValid_ReturnsList()
    {
        // Arrange
        var externalCredentialId = Guid.NewGuid();
        var requestId = Guid.NewGuid().ToString();
        var request = new CredentialRequestReceived(
            Id: requestId,
            Status: "APPROVED",
            IssuerDid: "did:web:example.org:issuer",
            HolderDid: "did:web:example.org:holder",
            DeliveryStatus: "DELIVERED",
            ExpirationDate: DateTime.UtcNow.AddDays(1).ToString("o"),
            ApprovedCredentials: [externalCredentialId.ToString()],
            MatchingCredentials: new List<Credential>(),
            RequestedCredentials: new List<RequestedCredentialsType>{
                new RequestedCredentialsType("vcdm11_jwt", "BpnCredential")
            });
        var response = new GetCredentialRequestReceivedResponse(1, new List<CredentialRequestReceived> { request });
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(response)));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        var result = await _sut.GetCredentialRequestsReceived("did:test", CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(requestId);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "{ \"message\": \"Framework test!\" }", "call to external system get-credential-requests-received-list failed with statuscode 409 - Message: { \"message\": \"Framework test!\" }")]
    [InlineData(HttpStatusCode.BadRequest, "{ \"test\": \"123\" }", "call to external system get-credential-requests-received-list failed with statuscode 400 - Message: { \"test\": \"123\" }")]
    public async Task GetCredentialRequestsReceived_WithConflict_ThrowsServiceException(HttpStatusCode statusCode, string? content, string message)
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(statusCode, new StringContent(content!));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);

        // Act
        async Task Act() => await _sut.GetCredentialRequestsReceived("did:test", CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion

    #region GetCredentialRequestsReceivedDetail

    [Fact]
    public async Task GetCredentialRequestsReceivedDetail_WithValid_ReturnsRequest()
    {
        // Arrange
        var externalCredentialId = Guid.NewGuid();
        var status = "APPROVED";
        var requestId = Guid.NewGuid().ToString();
        var request = new CredentialRequestReceived(
            Id: requestId,
            Status: status,
            IssuerDid: "did:web:example.org:issuer",
            HolderDid: "did:web:example.org:holder",
            DeliveryStatus: "DELIVERED",
            ExpirationDate: DateTime.UtcNow.AddDays(1).ToString("o"),
            ApprovedCredentials: [externalCredentialId.ToString()],
            MatchingCredentials: new List<Credential>(),
            RequestedCredentials: new List<RequestedCredentialsType>{
                new RequestedCredentialsType("vcdm11_jwt", "BpnCredential")
            });
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(request)));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        var result = await _sut.GetCredentialRequestsReceivedDetail(requestId, CancellationToken.None);

        // Assert
        result.Id.Should().Be(requestId);
        result.Status.Should().Be(status);
    }

    [Fact]
    public async Task GetCredentialRequestsReceivedDetail_WithNullResponse_ThrowsException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, new StringContent("null"));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        async Task Act() => await _sut.GetCredentialRequestsReceivedDetail("id1", CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("Response must contain a valid id");
    }

    #endregion

    #region CredentialRequestsReceivedAutoApprove

    [Fact]
    public async Task CredentialRequestsReceivedAutoApprove_WithValid_ReturnsStatus()
    {
        // Arrange
        var response = new RequestedCredentialAutoApproveResponse("id1", "id2", "APPROVED", null);
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(response)));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        var result = await _sut.CredentialRequestsReceivedAutoApprove("id1", CancellationToken.None);

        // Assert
        result.Should().Be("APPROVED");
    }

    [Fact]
    public async Task CredentialRequestsReceivedAutoApprove_WithReason_ThrowsException()
    {
        // Arrange
        var response = new RequestedCredentialAutoApproveResponse("id1", "id2", "faild", "The credential request has expired.");
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(response)));
        using var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        async Task Act() => await _sut.CredentialRequestsReceivedAutoApprove("id1", CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be("The credential request has expired.");
    }

    #endregion
}
