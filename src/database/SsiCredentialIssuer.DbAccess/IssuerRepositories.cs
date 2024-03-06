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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;

public class IssuerRepositories : IIssuerRepositories
{
    private readonly IssuerDbContext _dbContext;

    private static readonly IReadOnlyDictionary<Type, Func<IssuerDbContext, object>> Types = new Dictionary<Type, Func<IssuerDbContext, object>> {
        { typeof(ICompanySsiDetailsRepository), context => new CompanySsiDetailsRepository(context) },
        { typeof(ICredentialRepository), context => new CredentialRepository(context) },
        { typeof(IDocumentRepository), context => new DocumentRepository(context) },
        { typeof(IProcessStepRepository), context => new ProcessStepRepository(context) },
    }.ToImmutableDictionary();

    public IssuerRepositories(IssuerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public RepositoryType GetInstance<RepositoryType>()
    {
        Object? repository = default;

        if (Types.TryGetValue(typeof(RepositoryType), out var createFunc))
        {
            repository = createFunc(_dbContext);
        }

        return (RepositoryType)(repository ?? throw new ArgumentException($"unexpected type {typeof(RepositoryType).Name}", nameof(RepositoryType)));
    }

    /// <inheritdoc />
    public TEntity Attach<TEntity>(TEntity entity, Action<TEntity>? setOptionalParameters = null) where TEntity : class
    {
        var attachedEntity = _dbContext.Attach(entity).Entity;
        setOptionalParameters?.Invoke(attachedEntity);

        return attachedEntity;
    }

    public Task<int> SaveAsync()
    {
        try
        {
            return _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConflictException("while processing a concurrent update was saved to the database (reason could also be data to be deleted is no longer existing)", e);
        }
    }

    public void Clear() => _dbContext.ChangeTracker.Clear();
}
