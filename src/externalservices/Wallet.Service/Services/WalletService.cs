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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Services;

public class WalletService(IBasicAuthTokenService basicAuthTokenService, IOptions<WalletSettings> options)
    : IWalletService
{
    private const string NoIdErrorMessage = "Response must contain a valid id";
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly WalletSettings _settings = options.Value;

    public async Task<Guid> CreateCredential(JsonDocument payload, CancellationToken cancellationToken)
    {
        using var client = await basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_settings, cancellationToken);
        var data = new CreateCredentialRequest(_settings.WalletApplication, new CredentialPayload(payload));
        var result = await client.PostAsJsonAsync(_settings.CredentialCreationPath, data, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("create-credential", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async x => (false, await x.Content.ReadAsStringAsync().ConfigureAwait(ConfigureAwaitOptions.None)))
            .ConfigureAwait(false);
        var response = await result.Content.ReadFromJsonAsync<CreateCredentialResponse>(Options, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        if (response == null)
        {
            throw new ConflictException(NoIdErrorMessage);
        }

        return response.Id;
    }

    public async Task<string> SignCredential(Guid credentialId, CancellationToken cancellationToken)
    {
        using var client = await basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_settings, cancellationToken);
        var data = new SignCredentialRequest(new SignPayload(new SignUpdate("external", "jwt")));
        var result = await client.PatchAsJsonAsync(string.Format(_settings.SigningCredentialPath, credentialId), data, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("sign-credential", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async x => (false, await x.Content.ReadAsStringAsync().ConfigureAwait(ConfigureAwaitOptions.None)))
            .ConfigureAwait(false);
        var response = await result.Content.ReadFromJsonAsync<SignCredentialResponse>(Options, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        if (response is null)
        {
            throw new ServiceException(NoIdErrorMessage, true);
        }

        return response.Jwt;
    }

    public async Task<JsonDocument> GetCredential(Guid externalCredentialId, CancellationToken cancellationToken)
    {
        using var client = await basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_settings, cancellationToken);
        var result = await client.GetAsync(string.Format(_settings.SigningCredentialPath, externalCredentialId), cancellationToken)
            .CatchingIntoServiceExceptionFor("get-credential", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async x => (false, await x.Content.ReadAsStringAsync().ConfigureAwait(ConfigureAwaitOptions.None)))
            .ConfigureAwait(false);
        var response = await result.Content.ReadFromJsonAsync<GetCredentialResponse>(Options, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        if (response is null)
        {
            throw new ServiceException(NoIdErrorMessage, true);
        }

        return response.Credential;
    }

    public async Task<Guid> CreateCredentialForHolder(string holderWalletUrl, string clientId, string clientSecret, string credential, CancellationToken cancellationToken)
    {
        var authSettings = new BasicAuthSettings
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            TokenAddress = $"{holderWalletUrl}/oauth/token"
        };
        using var client = await basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(authSettings, cancellationToken);
        var data = new DeriveCredentialData(_settings.WalletApplication, new DeriveCredentialPayload(new DeriveCredential(credential)));
        var result = await client.PostAsJsonAsync(_settings.CredentialCreationPath, data, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("create-holder-credential", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async x => (false, await x.Content.ReadAsStringAsync().ConfigureAwait(ConfigureAwaitOptions.None)))
            .ConfigureAwait(false);
        var response = await result.Content.ReadFromJsonAsync<CreateCredentialResponse>(Options, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        if (response is null)
        {
            throw new ServiceException(NoIdErrorMessage, true);
        }

        return response.Id;
    }

    public async Task RevokeCredentialForIssuer(Guid externalCredentialId, CancellationToken cancellationToken)
    {
        using var client = await basicAuthTokenService.GetBasicAuthorizedClient<WalletService>(_settings, cancellationToken);
        var data = new RevokeCredentialRequest(new RevokePayload(true));
        await client.PatchAsJsonAsync(string.Format(_settings.RevokeCredentialPath, externalCredentialId), data, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("revoke-credential", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async x => (false, await x.Content.ReadAsStringAsync().ConfigureAwait(ConfigureAwaitOptions.None)))
            .ConfigureAwait(false);
    }
}
