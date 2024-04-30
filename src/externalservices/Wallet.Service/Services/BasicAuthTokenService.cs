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

using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Services;

public class BasicAuthTokenService : IBasicAuthTokenService
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IHttpClientFactory _httpClientFactory;

    public BasicAuthTokenService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HttpClient> GetBasicAuthorizedClient<T>(BasicAuthSettings settings, CancellationToken cancellationToken)
    {
        var tokenParameters = new GetBasicTokenSettings(
            $"{typeof(T).Name}Auth",
            settings.ClientId,
            settings.ClientSecret,
            settings.TokenAddress,
            "client_credentials");

        var token = await GetBasicTokenAsync(tokenParameters, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        var httpClient = _httpClientFactory.CreateClient(typeof(T).Name);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return httpClient;
    }

    private async Task<string?> GetBasicTokenAsync(GetBasicTokenSettings settings, CancellationToken cancellationToken)
    {
        var formParameters = new Dictionary<string, string>
        {
            { "grant_type", settings.GrantType }
        };
        using var content = new FormUrlEncodedContent(formParameters);
        using var authClient = _httpClientFactory.CreateClient(settings.HttpClientName);
        var authenticationString = $"{settings.ClientId}:{settings.ClientSecret}";
        var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));

        authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);

        var response = await authClient.PostAsync(settings.TokenAddress, content, cancellationToken)
            .CatchingIntoServiceExceptionFor("token-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);

        var responseObject = await response.Content.ReadFromJsonAsync<AuthResponse>(Options, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return responseObject?.AccessToken;
    }
}
