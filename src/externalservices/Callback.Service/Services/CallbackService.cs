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
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Callback.Service.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Callback.Service.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Callback.Service.Services;

public class CallbackService : ICallbackService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    private readonly ITokenService _tokenService;
    private readonly CallbackSettings _settings;

    public CallbackService(ITokenService tokenService, IOptions<CallbackSettings> options)
    {
        _tokenService = tokenService;
        _settings = options.Value;
    }

    public async Task TriggerCallback(string callbackUrl, IssuerResponseData responseData, CancellationToken cancellationToken)
    {
        var client = await _tokenService.GetAuthorizedClient<CallbackService>(_settings, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await client.PostAsJsonAsync($"{callbackUrl}", responseData, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("callback", HttpAsyncResponseMessageExtension.RecoverOptions.REQUEST_EXCEPTION)
            .ConfigureAwait(false);
    }
}
