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
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Renewal.App;

/// <summary>
/// Service to re-issue credentials that will expire in the day after.
/// </summary>
public class RenewalService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RenewalService> _logger;
    private readonly ICompanySsiDetailsRepository _companySsiDetailsRepository;

    /// <summary>
    /// Creates a new instance of <see cref="RenewalService"/>
    /// </summary>
    /// <param name="serviceScopeFactory">access to the services</param>
    /// <param name="logger">the logger</param>
    public RenewalService(IServiceScopeFactory serviceScopeFactory,
        ILogger<RenewalService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
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

                var credentialsAboutToExpire = companySsiDetailsRepository.GetCredentialsAboutToExpire(expirationDate);
                await CreateNewCredentials(credentialsAboutToExpire);
            }
            catch (Exception ex)
            {
                Environment.ExitCode = 1;
                _logger.LogError("Verified Credential re-issuance check failed with error: {Errors}", ex.Message);
            }
        }
    }

    private async Task CreateNewCredentials(IAsyncEnumerable<CredentialAboutToExpireData> credentialsAboutToExpire)
    {
        await foreach(var item in credentialsAboutToExpire)
        {
            _logger.LogInformation("HolderBpn: {0}", item.HolderBpn);
        }
    }
}
