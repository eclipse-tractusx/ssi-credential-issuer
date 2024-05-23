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

using Microsoft.AspNetCore.Authorization;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Security.Claims;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;

public class MandatoryIdentityClaimRequirement : IAuthorizationRequirement
{
    public MandatoryIdentityClaimRequirement(PolicyTypeId policyTypeId)
    {
        PolicyTypeId = policyTypeId;
    }

    public PolicyTypeId PolicyTypeId { get; }
}

public class MandatoryIdentityClaimHandler : AuthorizationHandler<MandatoryIdentityClaimRequirement>
{
    private readonly IClaimsIdentityDataBuilder _identityDataBuilder;
    private readonly ILogger<MandatoryIdentityClaimHandler> _logger;

    public MandatoryIdentityClaimHandler(IClaimsIdentityDataBuilder claimsIdentityDataBuilder, ILogger<MandatoryIdentityClaimHandler> logger)
    {
        _identityDataBuilder = claimsIdentityDataBuilder;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MandatoryIdentityClaimRequirement requirement)
    {
        if (_identityDataBuilder.Status == IClaimsIdentityDataBuilderStatus.Initial)
        {
            InitializeClaims(context.User);
        }

        if (_identityDataBuilder.Status == IClaimsIdentityDataBuilderStatus.Empty)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (requirement.PolicyTypeId switch
        {
            PolicyTypeId.ValidIdentity => _identityDataBuilder.IdentityId != string.Empty,
            PolicyTypeId.ValidBpn => !string.IsNullOrWhiteSpace(_identityDataBuilder.Bpnl),
            _ => throw new UnexpectedConditionException($"unexpected PolicyTypeId {requirement.PolicyTypeId}")
        })
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }

    private void InitializeClaims(ClaimsPrincipal principal)
    {
        var preferredUserName = principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.PreferredUserName)?.Value;
        var clientId = principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.ClientId)?.Value;
        if (preferredUserName == null && clientId == null)
        {
            _logger.LogInformation("Both preferred_user_name and client_id are null");
            _identityDataBuilder.Status = IClaimsIdentityDataBuilderStatus.Empty;
            return;
        }

        var bpnl = principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Bpn)?.Value;
        if (!string.IsNullOrWhiteSpace(bpnl)) // we only set the bpn if available, technical users don't have the bpn in the claims
        {
            _identityDataBuilder.AddBpnl(bpnl);
        }

        _identityDataBuilder.AddIdentityId(preferredUserName ?? clientId!);
        var isCompanyUser = Guid.TryParse(preferredUserName, out var companyUserId);
        if (isCompanyUser)
        {
            _identityDataBuilder.AddCompanyUserId(companyUserId);
        }

        _identityDataBuilder.AddIsServiceAccount(!isCompanyUser);
        _identityDataBuilder.Status = IClaimsIdentityDataBuilderStatus.Complete;
    }
}
