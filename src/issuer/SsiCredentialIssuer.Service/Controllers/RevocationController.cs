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

using Microsoft.AspNetCore.Mvc;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Extensions;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Models;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Controllers;

/// <summary>
/// Creates a new instance of <see cref="RevocationController"/>
/// </summary>
public static class RevocationController
{
    public static RouteGroupBuilder MapRevocationApi(this RouteGroupBuilder group)
    {
        var revocation = group.MapGroup("/revocation");

        revocation.MapPost("issuer/credentials/{credentialId}", ([FromRoute] Guid credentialId, CancellationToken cancellationToken, [FromServices] IRevocationBusinessLogic logic) => logic.RevokeCredential(credentialId, true, cancellationToken))
            .WithSwaggerDescription("Revokes an credential which was issued by the given issuer",
                "POST: api/revocation/issuer/credentials/{credentialId}",
                "Id of the credential that should be revoked")
            .RequireAuthorization(r =>
            {
                r.RequireRole("revoke_credentials_issuer");
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidBpn));
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidIdentity));
            })
            .WithDefaultResponses()
            .Produces(StatusCodes.Status200OK, typeof(Guid));
        revocation.MapPost("credentials/{credentialId}", ([FromRoute] Guid credentialId, CancellationToken cancellationToken, [FromServices] IRevocationBusinessLogic logic) => logic.RevokeCredential(credentialId, false, cancellationToken))
            .WithSwaggerDescription("Credential Revocation by holder",
                "POST: api/revocation/credentials/{credentialId}",
                "Id of the credential that should be revoked",
                "CancellationToken")
            .RequireAuthorization(r =>
            {
                r.RequireRole("revoke_credential");
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidBpn));
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidIdentity));
            })
            .WithDefaultResponses()
            .Produces(StatusCodes.Status200OK, typeof(Guid), contentType: Constants.JsonContentType);

        return group;
    }
}
