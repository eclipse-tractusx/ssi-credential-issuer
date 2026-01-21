/********************************************************************************
 * Copyright (c) 2025 Cofinity-X GmbH
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Extensions;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Models;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Services;

/// <summary>
/// Service to handle reissuance of expiring credentials
/// </summary>
public class ReissuanceService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ReissuanceService> _logger;
    private readonly ReissuanceServiceSettings _settings;

    /// <summary>
    /// Creates a new instance of <see cref="ReissuanceService"/>
    /// </summary>
    /// <param name="serviceScopeFactory">access to the services</param>
    /// <param name="logger">the logger</param>
    /// <param name="options">The options</param>
    public ReissuanceService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ReissuanceService> logger,
        IOptions<ReissuanceServiceSettings> options)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _settings = options.Value;
    }

    /// <summary>
    /// Executes the reissuance logic
    /// </summary>
    /// <param name="stoppingToken">Cancellation Token</param>
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Reissuance Service execution");

        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        using var processServiceScope = _serviceScopeFactory.CreateScope();
        var issuerBusinessLogic = processServiceScope.ServiceProvider.GetRequiredService<IIssuerBusinessLogic>();
        var dateTimeProvider = processServiceScope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        using var outerLoopScope = _serviceScopeFactory.CreateScope();
        var outerLoopRepositories = outerLoopScope.ServiceProvider.GetRequiredService<IIssuerRepositories>();

        var now = dateTimeProvider.OffsetNow;
        var expiredVcsToReissue = now.AddDays(_settings.ExpiredVcsToReissueInDays);

        var companySsiDetailsRepository = outerLoopRepositories.GetInstance<ICompanySsiDetailsRepository>();

        var credentials = await companySsiDetailsRepository.GetExpiryCredentials(expiredVcsToReissue).ToListAsync(stoppingToken).ConfigureAwait(false);

        _logger.LogInformation("Total number of credentials to be processed for reissuance: {Count}", credentials.Count);

        foreach (var credential in credentials)
        {
            await ProcessReissuance(credential, issuerBusinessLogic, outerLoopRepositories, stoppingToken).ConfigureAwait(false);
        }

        _logger.LogInformation("Reissuance Service execution completed successfully");
    }

    private async Task ProcessReissuance(
        ReissueCredential data,
        IIssuerBusinessLogic issuerBusinessLogic,
        IIssuerRepositories repositories,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing reissuance for credential ID: {Id}, Type: {Type}", data.Id, data.VerifiedCredentialTypeId);

            var credentialBase = JsonSerializer.Deserialize<Credential>(data.CredentialJson!.RootElement.GetRawText())!;
            var holderDid = credentialBase.CredentialSubject.Id;
            var holderUrl = ParseDidToUrl(holderDid);

            Guid newId;
            switch (data.VerifiedCredentialTypeId)
            {
                case VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER:
                    var bpnRequest = new CreateBpnCredentialRequest(
                        Holder: holderUrl,
                        BusinessPartnerNumber: data.Bpnl,
                        CallbackUrl: null,
                        TechnicalUserDetails: null
                    );
                    newId = await issuerBusinessLogic.CreateBpnCredential(bpnRequest, cancellationToken);
                    break;
                case VerifiedCredentialTypeId.MEMBERSHIP:
                    var membershipRequest = new CreateMembershipCredentialRequest(
                        Holder: holderUrl,
                        HolderBpn: data.Bpnl,
                        MemberOf: credentialBase.CredentialSubject.MemberOf,
                        CallbackUrl: null,
                        TechnicalUserDetails: null
                    );
                    newId = await issuerBusinessLogic.CreateMembershipCredential(membershipRequest, cancellationToken);
                    break;
                case VerifiedCredentialTypeId.DATA_EXCHANGE_GOVERNANCE_CREDENTIAL:
                    var frameworkRequest = new CreateFrameworkCredentialRequest(
                        Holder: holderUrl,
                        HolderBpn: data.Bpnl,
                        UseCaseFrameworkId: VerifiedCredentialTypeId.DATA_EXCHANGE_GOVERNANCE_CREDENTIAL,
                        UseCaseFrameworkVersionId: data.VerifiedCredentialExternalTypeDetailVersionId,
                        CallbackUrl: null,
                        TechnicalUserDetails: null
                    );
                    newId = await issuerBusinessLogic.CreateFrameworkCredentialBySystem(frameworkRequest, cancellationToken);
                    break;
                default:
                    _logger.LogWarning("Unsupported credential type for reissuance: {Type}", data.VerifiedCredentialTypeId);
                    return;
            }

            repositories.GetInstance<ICompanySsiDetailsRepository>().AttachAndModifyCompanySsiDetails(data.Id, null, c =>
            {
                c.ReissuedCredentialId = newId;
            });
            await repositories.SaveAsync().ConfigureAwait(false);
            _logger.LogInformation("Successfully reissued credential for ID: {Id}", data.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process reissuance for credential ID: {Id}", data.Id);
        }
    }

    private static string ParseDidToUrl(string did)
    {
        var parsedDid = did.Parse();
        if (parsedDid == null)
        {
            throw new InvalidOperationException("Invalid DID");
        }
        var docPath = "/.well-known/did.json";
        var path = Uri.UnescapeDataString(parsedDid.Id) + docPath;
        var didParts = parsedDid.Id.Split(':');

        if (didParts.Length > 1)
        {
            path = string.Join("/", didParts.Select(Uri.UnescapeDataString)) + "/did.json";
        }

        var uriBuilder = new UriBuilder
        {
            Scheme = "https",
            Host = path
        };
        return uriBuilder.ToString().TrimEnd('/');
    }
}
