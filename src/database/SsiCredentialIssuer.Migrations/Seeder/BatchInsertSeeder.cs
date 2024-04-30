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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Auditing;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Migrations.Seeder;

/// <summary>
/// Seeder to seed the base entities (those with an id as primary key)
/// </summary>
public class BatchInsertSeeder : ICustomSeeder
{
    private readonly IssuerDbContext _context;
    private readonly ILogger<BatchInsertSeeder> _logger;
    private readonly SeederSettings _settings;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger</param>
    /// <param name="options">The options</param>
    public BatchInsertSeeder(IssuerDbContext context, ILogger<BatchInsertSeeder> logger, IOptions<SeederSettings> options)
    {
        _context = context;
        _logger = logger;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public int Order => 1;

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!_settings.DataPaths.Any())
        {
            _logger.LogInformation("There a no data paths configured, therefore the {SeederName} will be skipped", nameof(BatchUpdateSeeder));
            return;
        }

        _logger.LogInformation("Start BaseEntityBatch Seeder");
        await SeedBaseEntity(cancellationToken);
        await SeedTable<CompanySsiProcessData>("company_ssi_process_datas", x => x.CompanySsiDetailId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await SeedTable<VerifiedCredentialTypeAssignedKind>("verified_credential_type_assigned_kinds", x => new { x.VerifiedCredentialTypeId, x.VerifiedCredentialTypeKindId }, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await SeedTable<VerifiedCredentialTypeAssignedUseCase>("verified_credential_type_assigned_use_cases", x => new { x.VerifiedCredentialTypeId, x.UseCaseId }, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await SeedTable<VerifiedCredentialTypeAssignedExternalType>("verified_credential_type_assigned_external_types", x => new { x.VerifiedCredentialTypeId, x.VerifiedCredentialExternalTypeId }, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        _logger.LogInformation("Finished BaseEntityBatch Seeder");
    }

    private async Task SeedBaseEntity(CancellationToken cancellationToken)
    {
        await SeedTableForBaseEntity<Document>("documents", cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await SeedTableForBaseEntity<UseCase>("use_cases", cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await SeedTableForBaseEntity<ProcessStep>("process_steps", cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await SeedTableForBaseEntity<Process>("processes", cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await SeedTableForBaseEntity<VerifiedCredentialExternalTypeDetailVersion>("verified_credential_external_type_detail_versions", cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await SeedTableForBaseEntity<CompanySsiDetail>("company_ssi_details", cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task SeedTableForBaseEntity<T>(string fileName, CancellationToken cancellationToken) where T : class, IBaseEntity
    {
        await SeedTable<T>(fileName, x => x.Id, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task SeedTable<T>(string fileName, Func<T, object> keySelector, CancellationToken cancellationToken) where T : class
    {
        _logger.LogInformation("Start seeding {Filename}", fileName);
        var additionalEnvironments = _settings.TestDataEnvironments ?? Enumerable.Empty<string>();
        var data = await SeederHelper.GetSeedData<T>(_logger, fileName, _settings.DataPaths, cancellationToken, additionalEnvironments.ToArray()).ConfigureAwait(ConfigureAwaitOptions.None);
        _logger.LogInformation("Found {ElementCount} data", data.Count);
        if (data.Any())
        {
            var typeName = typeof(T).Name;
            _logger.LogInformation("Started to Seed {TableName}", typeName);
            data = data.GroupJoin(_context.Set<T>(), keySelector, keySelector, (d, dbEntry) => new { d, dbEntry })
                .SelectMany(t => t.dbEntry.DefaultIfEmpty(), (t, x) => new { t, x })
                .Where(t => t.x == null)
                .Select(t => t.t.d).ToList();
            _logger.LogInformation("Seeding {DataCount} {TableName}", data.Count, typeName);
            await _context.Set<T>().AddRangeAsync(data, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            _logger.LogInformation("Seeded {TableName}", typeName);
        }
    }
}
