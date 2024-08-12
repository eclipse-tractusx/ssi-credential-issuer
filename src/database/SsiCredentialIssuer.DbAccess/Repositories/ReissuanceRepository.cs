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

using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;

public class ReissuanceRepository : IReissuanceRepository
{
    private readonly IssuerDbContext _dbContext;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dbContext">IssuerDbContext context.</param>
    public ReissuanceRepository(IssuerDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public void CreateReissuanceProcess(Guid id, Guid reissuedCredentialId)
    {
        var reissuanceProcess = new ReissuanceProcess(id, reissuedCredentialId);
        _dbContext.Reissuances.Add(reissuanceProcess);
    }

    public Guid GetCompanySsiDetailId(Guid companySsiDetaillId)
    {
        return _dbContext.Reissuances
            .Where(ssi => ssi.ReissuedCredentialId == companySsiDetaillId)
            .Select(ssi => ssi.Id).SingleOrDefault();
    }

    public bool IsReissuedCredential(Guid companySsiDetaillId)
    {
        return _dbContext.Reissuances
            .Where(ssi => ssi.ReissuedCredentialId == companySsiDetaillId)
            .Select(ssi => true).SingleOrDefault();
    }
}
