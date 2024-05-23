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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Web;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Extensions;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Models;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Controllers;

public static class CredentialController
{
    public static RouteGroupBuilder MapCredentialApi(this RouteGroupBuilder group)
    {
        var issuer = group.MapGroup("/credential");

        issuer.MapGet("{credentialId}", ([FromRoute] Guid credentialId, [FromServices] ICredentialBusinessLogic logic) => logic.GetCredentialDocument(credentialId))
            .WithSwaggerDescription("The endpoint enables users to download the credential (full json) of their own company.",
                "Example: GET: api/credential/{credentialId}")
            .RequireAuthorization(r =>
            {
                r.RequireRole("view_credential");
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidBpn));
            })
            .WithDefaultResponses()
            .Produces(StatusCodes.Status200OK, typeof(JsonDocument), Constants.JsonContentType)
            .Produces(StatusCodes.Status409Conflict, typeof(ErrorResponse), Constants.JsonContentType);

        issuer.MapGet("/documents/{documentId}", async ([FromRoute] Guid documentId, [FromServices] ICredentialBusinessLogic logic) =>
            {
                var (fileName, content, mediaType) = await logic.GetCredentialDocumentById(documentId);
                return Results.File(content, contentType: mediaType, fileDownloadName: fileName);
            })
            .WithSwaggerDescription("The endpoint enables users to download the credential (full json) of their own company.",
                "Example: GET: api/credential/documents/{documentId}")
            .RequireAuthorization(r =>
            {
                r.RequireRole("view_credential_requests");
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidIdentity));
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidBpn));
            })
            .WithDefaultResponses()
            .Produces(StatusCodes.Status200OK, typeof(FileContentResult), Constants.JsonContentType)
            .Produces(StatusCodes.Status409Conflict, typeof(ErrorResponse), Constants.JsonContentType);

        return issuer;
    }
}
