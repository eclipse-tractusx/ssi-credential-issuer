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
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Extensions;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Models;
using Constants = Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Models.Constants;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Controllers;

/// <summary>
/// Creates a new instance of <see cref="IssuerController"/>
/// </summary>
public static class IssuerController
{
    private const string RequestSsiRole = "request_ssicredential";
    private const string DecisionSsiRole = "decision_ssicredential";
    private const string ViewCredentialRequestRole = "view_credential_requests";

    public static RouteGroupBuilder MapIssuerApi(this RouteGroupBuilder group)
    {
        var issuer = group.MapGroup("/issuer");

        issuer.MapGet("useCaseParticipation", (IIssuerBusinessLogic logic) => logic.GetUseCaseParticipationAsync())
            .WithSwaggerDescription("Gets all use case frameworks and the participation status of the acting company",
                "Example: GET: api/issuer/useCaseParticipation")
            .RequireAuthorization(r =>
            {
                r.RequireRole("view_use_case_participation");
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidBpn));
            })
            .WithDefaultResponses()
            .Produces(StatusCodes.Status200OK, typeof(IEnumerable<UseCaseParticipationData>), Constants.JsonContentType)
            .Produces(StatusCodes.Status409Conflict, typeof(ErrorResponse), Constants.JsonContentType);

        issuer.MapGet("certificates", (IIssuerBusinessLogic logic) => logic.GetSsiCertificatesAsync())
            .WithSwaggerDescription("Gets all company certificate requests and their status",
                "Example: GET: api/issuer/certificates")
            .RequireAuthorization(r =>
            {
                r.RequireRole("view_certificates");
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidBpn));
            })
            .WithDefaultResponses()
            .Produces(StatusCodes.Status200OK, typeof(IEnumerable<CertificateParticipationData>), Constants.JsonContentType);

        issuer.MapGet("certificateTypes", (IIssuerBusinessLogic logic) => logic.GetCertificateTypes())
            .WithSwaggerDescription("Gets the certificate types for which the company can apply for",
                "Example: GET: api/issuer/certificateTypes")
            .RequireAuthorization(r =>
            {
                r.RequireRole(RequestSsiRole);
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidBpn));
            })
            .WithDefaultResponses()
            .Produces(StatusCodes.Status200OK, typeof(IEnumerable<VerifiedCredentialTypeId>), Constants.JsonContentType);

        issuer.MapGet(string.Empty, (IIssuerBusinessLogic logic, [FromQuery] int? page,
                [FromQuery] int? size,
                [FromQuery] CompanySsiDetailStatusId? companySsiDetailStatusId,
                [FromQuery] VerifiedCredentialTypeId? credentialTypeId,
                [FromQuery] CompanySsiDetailApprovalType? approvalType,
                [FromQuery] CompanySsiDetailSorting? sorting) => logic.GetCredentials(page ?? 0, size ?? 15,
                companySsiDetailStatusId, credentialTypeId, approvalType, sorting))
            .WithSwaggerDescription("Gets all outstanding, existing and inactive credentials",
                "Example: GET: /api/issuer",
                "The page to get",
                "Amount of entries",
                "OPTIONAL: Filter for the status",
                "OPTIONAL: The type of the credential that should be returned",
                "OPTIONAL: Search string for the company name",
                "Defines the sorting of the list")
            .RequireAuthorization(r => r.RequireRole(DecisionSsiRole))
            .WithDefaultResponses()
            .Produces(StatusCodes.Status200OK, typeof(IEnumerable<CredentialDetailData>), Constants.JsonContentType);

        issuer.MapGet("owned-credentials", (IIssuerBusinessLogic logic) => logic.GetCredentialsForBpn())
            .WithSwaggerDescription("Gets all outstanding, existing and inactive credentials for the company of the user",
                "Example: GET: /api/issuer/owned-credentials")
            .RequireAuthorization(r =>
            {
                r.RequireRole(ViewCredentialRequestRole);
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidBpn));
            })
            .WithDefaultResponses()
            .Produces(StatusCodes.Status200OK, typeof(IEnumerable<OwnedVerifiedCredentialData>), Constants.JsonContentType);

        issuer.MapPost("bpn", ([FromBody] CreateBpnCredentialRequest requestData, CancellationToken cancellationToken, IIssuerBusinessLogic logic) => logic.CreateBpnCredential(requestData, cancellationToken))
            .WithSwaggerDescription("Creates a bpn credential for the given data",
                "POST: api/issuer/bpn",
                "The request data containing information over the credential that should be created")
            .RequireAuthorization(r =>
            {
                r.RequireRole(RequestSsiRole);
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidIdentity));
            })
            .WithDefaultResponses()
            .Produces(StatusCodes.Status200OK, typeof(Guid), contentType: Constants.JsonContentType);

        issuer.MapPost("membership", ([FromBody] CreateMembershipCredentialRequest requestData, CancellationToken cancellationToken, IIssuerBusinessLogic logic) => logic.CreateMembershipCredential(requestData, cancellationToken))
            .WithSwaggerDescription("Creates a membership credential for the given data",
                "POST: api/issuer/membership",
                "The request data containing information over the credential that should be created")
            .RequireAuthorization(r =>
            {
                r.RequireRole(RequestSsiRole);
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidIdentity));
            })
            .WithDefaultResponses()
            .Produces(StatusCodes.Status200OK, typeof(Guid), contentType: Constants.JsonContentType);

        issuer.MapPost("framework", ([FromBody] CreateFrameworkCredentialRequest requestData, CancellationToken cancellationToken, IIssuerBusinessLogic logic) => logic.CreateFrameworkCredential(requestData, cancellationToken))
            .WithSwaggerDescription("Creates a framework credential for the given data",
                "POST: api/issuer/framework",
                "The request data containing information over the credential that should be created")
            .RequireAuthorization(r =>
            {
                r.RequireRole(RequestSsiRole);
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidIdentity));
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidBpn));
            })
            .WithDefaultResponses()
            .Produces(StatusCodes.Status200OK, typeof(Guid), contentType: Constants.JsonContentType);

        issuer.MapPut("{credentialId}/approval", async ([FromRoute] Guid credentialId, CancellationToken cancellationToken, IIssuerBusinessLogic logic) =>
            {
                await logic.ApproveCredential(credentialId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                return Results.NoContent();
            })
            .WithSwaggerDescription("Approves the given credential and triggers the verified credential creation",
                "PUT: api/issuer/{credentialId}/approval",
                "Id of the entry that should be approved",
                "Cancellation Token")
            .RequireAuthorization(r =>
            {
                r.RequireRole(DecisionSsiRole);
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidIdentity));
            })
            .WithDefaultResponses()
            .Produces(StatusCodes.Status204NoContent, contentType: Constants.JsonContentType)
            .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse), Constants.JsonContentType)
            .Produces(StatusCodes.Status409Conflict, typeof(ErrorResponse), Constants.JsonContentType);

        issuer.MapPut("{credentialId}/reject", async ([FromRoute] Guid credentialId, CancellationToken cancellationToken, IIssuerBusinessLogic logic) =>
            {
                await logic.RejectCredential(credentialId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
                return Results.NoContent();
            })
            .WithSwaggerDescription("Rejects the given credential",
                "PUT: api/issuer/{credentialId}/reject",
                "Id of the entry that should be rejected")
            .RequireAuthorization(r =>
            {
                r.RequireRole(DecisionSsiRole);
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidBpn));
                r.AddRequirements(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidIdentity));
            })
            .WithDefaultResponses()
            .Produces(StatusCodes.Status204NoContent, contentType: Constants.JsonContentType)
            .Produces(StatusCodes.Status404NotFound, typeof(ErrorResponse), Constants.JsonContentType)
            .Produces(StatusCodes.Status409Conflict, typeof(ErrorResponse), Constants.JsonContentType);

        return group;
    }
}
