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

using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;

public class IssuerRepositories(IssuerDbContext dbContext) :
    AbstractRepositories<IssuerDbContext>(dbContext),
    IIssuerRepositories
{
    protected override IReadOnlyDictionary<Type, Func<IssuerDbContext, object>> RepositoryTypes => Types;

    private static readonly IReadOnlyDictionary<Type, Func<IssuerDbContext, object>> Types = ImmutableDictionary.CreateRange(
    [
        CreateTypeEntry<ICompanySsiDetailsRepository>(context => new CompanySsiDetailsRepository(context)),
        CreateTypeEntry<ICredentialRepository>(context => new CredentialRepository(context)),
        CreateTypeEntry<IDocumentRepository>(context => new DocumentRepository(context)),
        CreateTypeEntry<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>(context => new ProcessStepRepository<Process, ProcessType<Process, ProcessTypeId>, ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>, ProcessStepType<Process, ProcessTypeId, ProcessStepTypeId>, ProcessTypeId, ProcessStepTypeId>(new IssuerProcessDbContextAccess(context))),
    ]);
}
