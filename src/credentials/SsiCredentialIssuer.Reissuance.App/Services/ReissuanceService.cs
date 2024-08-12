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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Handlers;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Credential.Library.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Credential.Library.Context;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Services;

/// <summary>
/// Service to re-issue credentials that will expire in the day after.
/// </summary>
public class ReissuanceService : IReissuanceService
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ReissuanceService> _logger;
    private readonly ICredentialIssuerHandler _credentialIssuerHandler;
    /// <summary>
    /// Creates a new instance of <see cref="ReissuanceService"/>
    /// </summary>
    /// <param name="serviceScopeFactory">access to the services</param>
    /// <param name="credentialIssuerHandler">access to the credential issuer handler service</param>
    /// <param name="logger">the logger</param>
    public ReissuanceService(IServiceScopeFactory serviceScopeFactory,
        ICredentialIssuerHandler credentialIssuerHandler,
        ILogger<ReissuanceService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _credentialIssuerHandler = credentialIssuerHandler;
        _logger = logger;
    }

    /// <summary>
    /// Handles the process of re-issuing new verifiable credentias
    /// </summary>
    /// <param name="stoppingToken">Cancellation Token</param>public async Task ExecuteAsync(CancellationToken stoppingToken)
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var processServiceScope = _serviceScopeFactory.CreateScope();
                var dateTimeProvider = processServiceScope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
                var repositories = processServiceScope.ServiceProvider.GetRequiredService<IIssuerRepositories>();
                var expirationDate = dateTimeProvider.OffsetNow.AddDays(1);
                var companySsiDetailsRepository = repositories.GetInstance<ICompanySsiDetailsRepository>();
                var credentialIssuerHandler = processServiceScope.ServiceProvider.GetRequiredService<ICredentialIssuerHandler>();
                var credentialsAboutToExpire = companySsiDetailsRepository.GetCredentialsAboutToExpire(expirationDate);
                await ProcessCredentials(credentialsAboutToExpire, dateTimeProvider);
            }
            catch (Exception ex)
            {
                Environment.ExitCode = 1;
                _logger.LogError("Verified Credential re-issuance check failed with error: {Errors}", ex.Message);
            }
        }
    }

    private async Task ProcessCredentials(IAsyncEnumerable<CredentialAboutToExpireData> credentialsAboutToExpire, IDateTimeProvider dateTimeProvider)
    {
        await foreach(var credential in credentialsAboutToExpire)
        {
            var expirationDate = dateTimeProvider.OffsetNow.AddMonths(12);

            var schemaData = CreateNewCredential(credential, expirationDate);

            await _credentialIssuerHandler.HandleCredentialProcessCreation(new IssuerCredentialRequest(
                credential.Id,
                credential.HolderBpn,
                credential.VerifiedCredentialTypeKindId,
                credential.VerifiedCredentialTypeId,
                expirationDate,
                credential.IdentityId,
                schemaData,
                credential.WalletUrl,
                credential.DetailVersionId,
                credential.CallbackUrl
            ));

        }
    }

    private static string CreateNewCredential(CredentialAboutToExpireData credential, DateTimeOffset expirationDate)
    {
        string schemaData;
        if (credential.VerifiedCredentialTypeKindId == VerifiedCredentialTypeKindId.BPN)
        {
            schemaData = CreateBpnCredential(credential, credential.Schema, expirationDate);
        }
        else
        {
            schemaData = CreateMembershipCredential(credential, credential.Schema, expirationDate);
        }
        return schemaData;
    }

    private static string CreateBpnCredential(CredentialAboutToExpireData credential, JsonDocument schema, DateTimeOffset expirationDate)
    {
        var bpnAboutToExpire = credential.Schema.Deserialize<BpnCredential>();
        var bpnCredential = new BpnCredential(
            Guid.NewGuid(),
            CredentialContext.Context,
            bpnAboutToExpire!.Type,
            bpnAboutToExpire.Name,
            bpnAboutToExpire.Description,
            DateTimeOffset.UtcNow,
            expirationDate,
            bpnAboutToExpire.Issuer,
            bpnAboutToExpire.CredentialSubject,
            bpnAboutToExpire.CredentialStatus);

        return JsonSerializer.Serialize(bpnCredential, Options);
    }

    private static string CreateMembershipCredential(CredentialAboutToExpireData credential, JsonDocument schema, DateTimeOffset expirationDate)
    {
        var membershipAboutToExpire = credential.Schema.Deserialize<MembershipCredential>();
        var membershipCredential = new MembershipCredential(
            Guid.NewGuid(),
            CredentialContext.Context,
            membershipAboutToExpire!.Type,
            membershipAboutToExpire.Name,
            membershipAboutToExpire.Description,
            DateTimeOffset.UtcNow,
            expirationDate,
            membershipAboutToExpire.Issuer,
            membershipAboutToExpire.CredentialSubject,
            membershipAboutToExpire.CredentialStatus);

        return JsonSerializer.Serialize(membershipCredential, Options);
    }
}
