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
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Expiry.App.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Services;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Expiry.App;

/// <summary>
/// Service to delete the pending and inactive documents as well as the depending consents from the database
/// </summary>
public class ExpiryCheckService
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ExpiryCheckService> _logger;
    private readonly ExpiryCheckServiceSettings _settings;

    /// <summary>
    /// Creates a new instance of <see cref="ExpiryCheckService"/>
    /// </summary>
    /// <param name="serviceScopeFactory">access to the services</param>
    /// <param name="logger">the logger</param>
    /// <param name="options">The options</param>
    public ExpiryCheckService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ExpiryCheckService> logger,
        IOptions<ExpiryCheckServiceSettings> options)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _settings = options.Value;
    }

    /// <summary>
    /// Handles the
    /// </summary>
    /// <param name="stoppingToken">Cancellation Token</param>
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var processServiceScope = _serviceScopeFactory.CreateScope();
                var repositories = processServiceScope.ServiceProvider.GetRequiredService<IIssuerRepositories>();
                var dateTimeProvider = processServiceScope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
                var portalService = processServiceScope.ServiceProvider.GetRequiredService<IPortalService>();

                using var outerLoopScope = _serviceScopeFactory.CreateScope();
                var outerLoopRepositories = outerLoopScope.ServiceProvider.GetRequiredService<IIssuerRepositories>();

                var now = dateTimeProvider.OffsetNow;
                var companySsiDetailsRepository = repositories.GetInstance<ICompanySsiDetailsRepository>();
                var processStepRepository = repositories.GetInstance<IProcessStepRepository>();
                var inactiveVcsToDelete = now.AddDays(-(_settings.InactiveVcsToDeleteInWeeks * 7));
                var expiredVcsToDelete = now.AddMonths(-_settings.ExpiredVcsToDeleteInMonth);

                var credentials = outerLoopRepositories.GetInstance<ICompanySsiDetailsRepository>()
                    .GetExpiryData(now, inactiveVcsToDelete, expiredVcsToDelete);
                await foreach (var credential in credentials.WithCancellation(stoppingToken).ConfigureAwait(false))
                {
                    await ProcessCredentials(credential, companySsiDetailsRepository, repositories, portalService, processStepRepository, stoppingToken).ConfigureAwait(ConfigureAwaitOptions.None);
                }
            }
            catch (Exception ex)
            {
                Environment.ExitCode = 1;
                _logger.LogError("Verified Credential expiry check failed with error: {Errors}", ex.Message);
            }
        }
    }

    private static async Task ProcessCredentials(
        CredentialExpiryData data,
        ICompanySsiDetailsRepository companySsiDetailsRepository,
        IIssuerRepositories repositories,
        IPortalService portalService,
        IProcessStepRepository processStepRepository,
        CancellationToken cancellationToken)
    {
        if (data.ScheduleData.IsVcToDelete)
        {
            companySsiDetailsRepository.RemoveSsiDetail(data.Id, data.Bpnl, data.RequesterId);
        }
        else if (data.ScheduleData.IsVcToDecline)
        {
            HandleDecline(data.Id, companySsiDetailsRepository, processStepRepository);
        }
        else
        {
            await HandleNotification(data, companySsiDetailsRepository, portalService, cancellationToken).ConfigureAwait(false);
        }

        // Saving here to make sure the each credential is handled by there own 
        await repositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private static void HandleDecline(
        Guid credentialId,
        ICompanySsiDetailsRepository companySsiDetailsRepository,
        IProcessStepRepository processStepRepository)
    {
        var processId = processStepRepository.CreateProcess(ProcessTypeId.DECLINE_CREDENTIAL).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.REVOKE_CREDENTIAL, ProcessStepStatusId.TODO, processId);
        companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(credentialId, c =>
            {
                c.ProcessId = null;
            },
            c =>
            {
                c.ProcessId = processId;
            });
    }

    private static async ValueTask HandleNotification(
        CredentialExpiryData data,
        ICompanySsiDetailsRepository companySsiDetailsRepository,
        IPortalService portalService,
        CancellationToken cancellationToken)
    {
        var newExpiryCheckTypeId = data.ScheduleData switch
        {
            { IsOneDayNotification: true } => ExpiryCheckTypeId.ONE_DAY,
            { IsTwoWeeksNotification: true } => ExpiryCheckTypeId.TWO_WEEKS,
            { IsOneMonthNotification: true } => ExpiryCheckTypeId.ONE_MONTH,
            _ => throw new UnexpectedConditionException("one of IsVcToDelete, IsOneDayNotification, IsTwoWeeksNotification, IsOneMonthNotification, IsVcToDecline is expected to be true")
        };

        companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(
            data.Id,
            csd =>
            {
                csd.ExpiryCheckTypeId = data.ExpiryCheckTypeId;
            },
            csd =>
            {
                csd.ExpiryCheckTypeId = newExpiryCheckTypeId;
            });

        var content = JsonSerializer.Serialize(new
        {
            Type = data.VerifiedCredentialExternalTypeId,
            ExpiryDate = data.ExpiryDate?.ToString("O") ?? throw new ConflictException("Expiry Date must be set here"),
            Version = data.DetailVersion,
            CredentialId = data.Id,
            ExpiryCheckTypeId = newExpiryCheckTypeId
        }, Options);

        if (Guid.TryParse(data.RequesterId, out var requesterId))
        {
            await portalService.AddNotification(content, requesterId, NotificationTypeId.CREDENTIAL_EXPIRY,
                cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            var typeValue = data.VerifiedCredentialExternalTypeId.GetEnumValue() ??
                            throw new UnexpectedConditionException(
                                $"VerifiedCredentialType {data.VerifiedCredentialExternalTypeId} does not exists");
            var mailParameters = new MailParameter[]
            {
                new("typeId", typeValue), new("version", data.DetailVersion ?? "no version"),
                new("expiryDate",
                    data.ExpiryDate?.ToString("dd MMMM yyyy") ?? throw new ConflictException("Expiry Date must be set here"))
            };
            await portalService.TriggerMail("CredentialExpiry", requesterId, mailParameters, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        }
    }
}
