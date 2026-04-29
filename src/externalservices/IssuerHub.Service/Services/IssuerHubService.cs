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
using Org.Eclipse.TractusX.SsiCredentialIssuer.IssuerHub.Service.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.IssuerHub.Service.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.IssuerHub.Service.Services;

public class IssuerHubService : IIssuerHubService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly HttpClient _httpClient;
    private readonly IssuerHubSettings _settings;

    public IssuerHubService(IHttpClientFactory httpClientFactory, IOptions<IssuerHubSettings> options)
    {
        _settings = options.Value;
        _httpClient = httpClientFactory.CreateClient(nameof(IssuerHubService));
    }

    public async Task CreateParticipantContext(CreateParticipantContextRequest request, CancellationToken cancellationToken)
    {
        await _httpClient.PostAsJsonAsync(
                $"{_settings.IdentityBasePath}/v1alpha/participants",
                request,
                JsonOptions,
                cancellationToken)
            .CatchingIntoServiceExceptionFor("create-participant-context", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async x => (false, await x.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)))
            .ConfigureAwait(false);
    }

    public async Task ActivateParticipantContext(string participantContextId, CancellationToken cancellationToken)
    {
        await _httpClient.PostAsync(
                $"{_settings.IdentityBasePath}/v1alpha/participants/{Uri.EscapeDataString(participantContextId)}/state?isActive=true",
                null,
                cancellationToken)
            .CatchingIntoServiceExceptionFor("activate-participant-context", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async x => (false, await x.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)))
            .ConfigureAwait(false);
    }

    public async Task CreateAttestationDefinition(CreateAttestationDefinitionRequest request, CancellationToken cancellationToken)
    {
        var participantContextId = Uri.EscapeDataString(_settings.ParticipantContextId);
        await _httpClient.PostAsJsonAsync(
                $"{_settings.IssuerAdminBasePath}/v1alpha/participants/{participantContextId}/attestations",
                request,
                JsonOptions,
                cancellationToken)
            .CatchingIntoServiceExceptionFor("create-attestation-definition", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async x => (false, await x.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)))
            .ConfigureAwait(false);
    }

    public async Task CreateCredentialDefinition(CreateCredentialDefinitionRequest request, CancellationToken cancellationToken)
    {
        var participantContextId = Uri.EscapeDataString(_settings.ParticipantContextId);
        await _httpClient.PostAsJsonAsync(
                $"{_settings.IssuerAdminBasePath}/v1alpha/participants/{participantContextId}/credentialdefinitions",
                request,
                JsonOptions,
                cancellationToken)
            .CatchingIntoServiceExceptionFor("create-credential-definition", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async x => (false, await x.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)))
            .ConfigureAwait(false);
    }

    public async Task RegisterHolder(RegisterHolderRequest request, CancellationToken cancellationToken)
    {
        var participantContextId = Uri.EscapeDataString(_settings.ParticipantContextId);
        await _httpClient.PostAsJsonAsync(
                $"{_settings.IssuerAdminBasePath}/v1alpha/participants/{participantContextId}/holders",
                request,
                JsonOptions,
                cancellationToken)
            .CatchingIntoServiceExceptionFor("register-holder", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async x => (false, await x.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)))
            .ConfigureAwait(false);
    }

    public async Task TriggerCredentialOffer(TriggerCredentialOfferRequest request, CancellationToken cancellationToken)
    {
        var participantContextId = Uri.EscapeDataString(_settings.ParticipantContextId);
        await _httpClient.PostAsJsonAsync(
                $"{_settings.IssuerAdminBasePath}/v1alpha/participants/{participantContextId}/credentials/offer",
                request,
                JsonOptions,
                cancellationToken)
            .CatchingIntoServiceExceptionFor("trigger-credential-offer", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async x => (false, await x.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)))
            .ConfigureAwait(false);
    }

    public async Task RevokeCredential(string credentialId, CancellationToken cancellationToken)
    {
        var participantContextId = Uri.EscapeDataString(_settings.ParticipantContextId);
        await _httpClient.PostAsync(
                $"{_settings.IssuerAdminBasePath}/v1alpha/participants/{participantContextId}/credentials/{Uri.EscapeDataString(credentialId)}/revoke",
                null,
                cancellationToken)
            .CatchingIntoServiceExceptionFor("revoke-credential", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async x => (false, await x.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)))
            .ConfigureAwait(false);
    }

    public async Task SuspendCredential(string credentialId, CancellationToken cancellationToken)
    {
        var participantContextId = Uri.EscapeDataString(_settings.ParticipantContextId);
        await _httpClient.PostAsync(
                $"{_settings.IssuerAdminBasePath}/v1alpha/participants/{participantContextId}/credentials/{Uri.EscapeDataString(credentialId)}/suspend",
                null,
                cancellationToken)
            .CatchingIntoServiceExceptionFor("suspend-credential", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async x => (false, await x.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)))
            .ConfigureAwait(false);
    }

    public async Task ResumeCredential(string credentialId, CancellationToken cancellationToken)
    {
        var participantContextId = Uri.EscapeDataString(_settings.ParticipantContextId);
        await _httpClient.PostAsync(
                $"{_settings.IssuerAdminBasePath}/v1alpha/participants/{participantContextId}/credentials/{Uri.EscapeDataString(credentialId)}/resume",
                null,
                cancellationToken)
            .CatchingIntoServiceExceptionFor("resume-credential", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async x => (false, await x.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)))
            .ConfigureAwait(false);
    }

    public async Task<CredentialStatusResponse> GetCredentialStatus(string credentialId, CancellationToken cancellationToken)
    {
        var participantContextId = Uri.EscapeDataString(_settings.ParticipantContextId);
        var result = await _httpClient.GetAsync(
                $"{_settings.IssuerAdminBasePath}/v1alpha/participants/{participantContextId}/credentials/{Uri.EscapeDataString(credentialId)}/status",
                cancellationToken)
            .CatchingIntoServiceExceptionFor("get-credential-status", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async x => (false, await x.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None)))
            .ConfigureAwait(false);

        var response = await result.Content.ReadFromJsonAsync<CredentialStatusResponse>(JsonOptions, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        if (response is null)
        {
            throw new ServiceException("Response must contain a valid credential status", true);
        }

        return response;
    }
}
