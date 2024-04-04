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
            EncrptionConfigIndex = 0
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
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        var result = await _sut.CreateCredential(payload, CancellationToken.None);

        // Assert
        httpMessageHandlerMock.RequestMessage.Should().Match<HttpRequestMessage>(x =>
            x.Content is JsonContent &&
            (x.Content as JsonContent)!.ObjectType == typeof(CreateCredentialRequest) &&
            ((x.Content as JsonContent)!.Value as CreateCredentialRequest)!.Application == "catena-x-portal"
        );
        result.Should().Be(id);
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
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);

        // Act
        Task Act() => _sut.CreateCredential(payload, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion

    #region SignCredential

    [Fact]
    public async Task SignCredential_WithValid_DoesNotThrowException()
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        const string jwt = "thisisonlyatestexample";
        var response = new SignCredentialResponse(jwt);
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(response)));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        var result = await _sut.SignCredential(credentialId, CancellationToken.None);

        // Assert
        httpMessageHandlerMock.RequestMessage.Should().Match<HttpRequestMessage>(x =>
            x.Content is JsonContent &&
            (x.Content as JsonContent)!.ObjectType == typeof(SignCredentialRequest) &&
            ((x.Content as JsonContent)!.Value as SignCredentialRequest)!.Payload.Sign.ProofMechanism == "external" &&
            ((x.Content as JsonContent)!.Value as SignCredentialRequest)!.Payload.Sign.ProofType == "jwt"
        );
        result.Should().Be(jwt);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "{ \"message\": \"Framework test!\" }", "call to external system sign-credential failed with statuscode 409 - Message: { \"message\": \"Framework test!\" }")]
    [InlineData(HttpStatusCode.BadRequest, "{ \"test\": \"123\" }", "call to external system sign-credential failed with statuscode 400 - Message: { \"test\": \"123\" }")]
    [InlineData(HttpStatusCode.BadRequest, "this is no json", "call to external system sign-credential failed with statuscode 400 - Message: this is no json")]
    [InlineData(HttpStatusCode.Forbidden, null, "call to external system sign-credential failed with statuscode 403")]
    public async Task SignCredential_WithConflict_ThrowsServiceExceptionWithErrorContent(HttpStatusCode statusCode, string? content, string message)
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        var httpMessageHandlerMock = content == null
            ? new HttpMessageHandlerMock(statusCode)
            : new HttpMessageHandlerMock(statusCode, new StringContent(content));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);

        // Act
        Task Act() => _sut.SignCredential(credentialId, CancellationToken.None);

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
        var httpClient = new HttpClient(httpMessageHandlerMock)
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
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);

        // Act
        Task Act() => _sut.GetCredential(credentialId, CancellationToken.None);

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
        var httpClient = new HttpClient(httpMessageHandlerMock)
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
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(A<BasicAuthSettings>._, A<CancellationToken>._)).Returns(httpClient);

        // Act
        Task Act() => _sut.CreateCredentialForHolder("https://example.org", "test", "testSec", "testCred", CancellationToken.None);

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
        var httpClient = new HttpClient(httpMessageHandlerMock)
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
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(A<BasicAuthSettings>._, A<CancellationToken>._)).Returns(httpClient);

        // Act
        Task Act() => _sut.RevokeCredentialForIssuer(Guid.NewGuid(), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion

    #region RevokeCredentialForHolder

    [Fact]
    public async Task RevokeCredentialForHolder_WithValid_DoesNotThrowException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = new CreateCredentialResponse(id);
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.OK, new StringContent(JsonSerializer.Serialize(response)));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(A<BasicAuthSettings>._, A<CancellationToken>._))
            .Returns(httpClient);

        // Act
        await _sut.RevokeCredentialForHolder("https://test.de", "test123", "cl1", Guid.NewGuid(), CancellationToken.None);

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
    public async Task RevokeCredentialForHolder_WithConflict_ThrowsServiceExceptionWithErrorContent(HttpStatusCode statusCode, string? content, string message)
    {
        // Arrange
        var httpMessageHandlerMock = content == null
            ? new HttpMessageHandlerMock(statusCode)
            : new HttpMessageHandlerMock(statusCode, new StringContent(content));
        var httpClient = new HttpClient(httpMessageHandlerMock)
        {
            BaseAddress = new Uri("https://base.address.com")
        };
        A.CallTo(() => _basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(A<BasicAuthSettings>._, A<CancellationToken>._)).Returns(httpClient);

        // Act
        Task Act() => _sut.RevokeCredentialForHolder("https://test.de", "test123", "cl1", Guid.NewGuid(), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion
}
