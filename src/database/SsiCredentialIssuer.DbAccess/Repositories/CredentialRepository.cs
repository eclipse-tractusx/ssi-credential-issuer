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
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;

public class CredentialRepository : ICredentialRepository
{
    private readonly IssuerDbContext _dbContext;

    public CredentialRepository(IssuerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Guid?> GetWalletCredentialId(Guid credentialId) =>
        _dbContext.CompanySsiDetails.Where(x => x.Id == credentialId)
            .Select(x => x.ExternalCredentialId)
            .SingleOrDefaultAsync();

    public Task<(HolderWalletData HolderWalletData, string? Credential, EncryptionTransformationData EncryptionInformation)> GetCredentialData(Guid credentialId) =>
        _dbContext.CompanySsiDetails
            .Where(x => x.Id == credentialId)
            .Select(x => new ValueTuple<HolderWalletData, string?, EncryptionTransformationData>(
                new HolderWalletData(x.CompanySsiProcessData!.HolderWalletUrl, x.CompanySsiProcessData.ClientId),
                x.Credential,
                new EncryptionTransformationData(x.CompanySsiProcessData!.ClientSecret, x.CompanySsiProcessData.InitializationVector, x.CompanySsiProcessData.EncryptionMode)))
            .SingleOrDefaultAsync();

    public Task<(bool Exists, Guid CredentialId)> GetDataForProcessId(Guid processId) =>
        _dbContext.CompanySsiDetails
            .Where(c => c.ProcessId == processId)
            .Select(c => new ValueTuple<bool, Guid>(true, c.Id))
            .SingleOrDefaultAsync();

    public Task<(VerifiedCredentialTypeKindId CredentialTypeKindId, JsonDocument Schema)> GetCredentialStorageInformationById(Guid credentialId) =>
        _dbContext.CompanySsiDetails
            .Where(c => c.Id == credentialId)
            .Select(c => new ValueTuple<VerifiedCredentialTypeKindId, JsonDocument>(c.CompanySsiProcessData!.CredentialTypeKindId, c.CompanySsiProcessData.Schema))
            .SingleOrDefaultAsync();

    public Task<(Guid? ExternalCredentialId, VerifiedCredentialTypeKindId KindId)> GetExternalCredentialAndKindId(Guid credentialId) =>
        _dbContext.CompanySsiDetails
            .Where(c => c.Id == credentialId)
            .Select(c => new ValueTuple<Guid?, VerifiedCredentialTypeKindId>(c.ExternalCredentialId, c.VerifiedCredentialType!.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId))
            .SingleOrDefaultAsync();
}
