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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Auditing.Identity;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;

public class IdentityIdService : IIdentityIdService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityIdService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string IdentityId => GetIdentityId();

    private string GetIdentityId()
    {
        var preferredUserName = _httpContextAccessor?.HttpContext?.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.PreferredUserName)?.Value ??
            throw new UnexpectedConditionException("Username must be set here");
        if (Guid.TryParse(preferredUserName, out var identityId))
        {
            return identityId.ToString();
        }

        throw new UnexpectedConditionException("Username must be a uuid");
    }
}
