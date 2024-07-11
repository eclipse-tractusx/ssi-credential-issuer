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
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Migrations.Seeder;

/// <summary>
/// Seeder to seed the base entities (those with an id as primary key)
/// </summary>
public class BatchUpdateSeeder : ICustomSeeder
{
    private readonly IssuerDbContext _context;
    private readonly ILogger<BatchUpdateSeeder> _logger;
    private readonly SeederSettings _settings;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger</param>
    /// <param name="options">The options</param>
    public BatchUpdateSeeder(IssuerDbContext context, ILogger<BatchUpdateSeeder> logger, IOptions<SeederSettings> options)
    {
        _context = context;
        _logger = logger;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public int Order => 2;

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!_settings.DataPaths.Any())
        {
            _logger.LogInformation("There a no data paths configured, therefore the {SeederName} will be skipped", nameof(BatchUpdateSeeder));
            return;
        }

        _logger.LogInformation("Start BaseEntityBatch Seeder");

        await SeedTable<UseCase>("use_cases",
            x => x.Id,
            x => x.dataEntity.Name != x.dbEntity.Name || x.dataEntity.Shortname != x.dbEntity.Shortname,
            (dbEntry, entry) =>
            {
                dbEntry.Name = entry.Name;
                dbEntry.Shortname = entry.Shortname;
            },
            cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await SeedTable<VerifiedCredentialExternalTypeDetailVersion>("verified_credential_external_type_detail_versions",
            x => x.Id,
            x => x.dataEntity.Template != x.dbEntity.Template || x.dataEntity.Expiry != x.dbEntity.Expiry || x.dataEntity.ValidFrom != x.dbEntity.ValidFrom || x.dataEntity.Version != x.dbEntity.Version,
            (dbEntry, entry) =>
            {
                dbEntry.Template = entry.Template;
                dbEntry.Expiry = entry.Expiry;
                dbEntry.ValidFrom = entry.ValidFrom;
                dbEntry.Version = entry.Version;
            },
            cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        _logger.LogInformation("Finished BaseEntityBatch Seeder");
    }

    private async Task SeedTable<T>(string fileName, Func<T, object> keySelector, Func<(T dataEntity, T dbEntity), bool> whereClause, Action<T, T> updateEntries, CancellationToken cancellationToken) where T : class
    {
        _logger.LogInformation("Start seeding {Filename}", fileName);
        var additionalEnvironments = _settings.TestDataEnvironments ?? Enumerable.Empty<string>();
        var data = await SeederHelper.GetSeedData<T>(_logger, fileName, _settings.DataPaths, cancellationToken, additionalEnvironments.ToArray()).ConfigureAwait(ConfigureAwaitOptions.None);
        _logger.LogInformation("Found {ElementCount} data", data.Count);
        if (data.Any())
        {
            var typeName = typeof(T).Name;
            var entriesForUpdate = data
                .Join(_context.Set<T>(), keySelector, keySelector, (dataEntry, dbEntry) => (DataEntry: dataEntry, DbEntry: dbEntry))
                .Where(whereClause.Invoke)
                .ToList();
            if (entriesForUpdate.Any())
            {
                _logger.LogInformation("Started to Update {EntryCount} entries of {TableName}", entriesForUpdate.Count, typeName);
                foreach (var entry in entriesForUpdate)
                {
                    updateEntries.Invoke(entry.DbEntry, entry.DataEntry);
                }

                _logger.LogInformation("Updated {TableName}", typeName);
            }
        }
    }
}
