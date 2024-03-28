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
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Services;

public class PortalService : IPortalService
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly ITokenService _tokenService;
    private readonly PortalSettings _settings;

    public PortalService(ITokenService tokenService, IOptions<PortalSettings> options)
    {
        _tokenService = tokenService;
        _settings = options.Value;
    }

    public async Task AddNotification(string content, Guid requester, NotificationTypeId notificationTypeId, CancellationToken cancellationToken)
    {
        var client = await _tokenService.GetAuthorizedClient<PortalService>(_settings, cancellationToken).ConfigureAwait(false);
        var data = new NotificationRequest(requester, content, notificationTypeId);
        await client.PostAsJsonAsync("api/notifications/management", data, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("notification", HttpAsyncResponseMessageExtension.RecoverOptions.REQUEST_EXCEPTION)
            .ConfigureAwait(false);
    }

    public async Task TriggerMail(string template, Guid requester, IEnumerable<MailParameter> mailParameters, CancellationToken cancellationToken)
    {
        var client = await _tokenService.GetAuthorizedClient<PortalService>(_settings, cancellationToken).ConfigureAwait(false);
        var data = new MailData(requester, template, mailParameters);
        await client.PostAsJsonAsync("api/administration/mail", data, Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("mail", HttpAsyncResponseMessageExtension.RecoverOptions.REQUEST_EXCEPTION)
            .ConfigureAwait(false);
    }
}
