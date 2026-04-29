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

using Org.Eclipse.TractusX.SsiCredentialIssuer.IssuerHub.Service.Models;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.IssuerHub.Service.Services;

/// <summary>
/// Client for the Eclipse Tractus-X Identity Hub Issuer Service Admin API.
/// </summary>
public interface IIssuerHubService
{
    /// <summary>
    /// Creates a new participant context.
    /// POST /v1alpha/participants
    /// </summary>
    Task CreateParticipantContext(CreateParticipantContextRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Activates a participant context.
    /// POST /v1alpha/participants/{participantContextId}/state?isActive=true
    /// </summary>
    Task ActivateParticipantContext(string participantContextId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates an attestation definition for the issuer participant.
    /// POST /v1alpha/participants/{participantContextId}/attestations
    /// </summary>
    Task CreateAttestationDefinition(CreateAttestationDefinitionRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a credential definition for the issuer participant.
    /// POST /v1alpha/participants/{participantContextId}/credentialdefinitions
    /// </summary>
    Task CreateCredentialDefinition(CreateCredentialDefinitionRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Registers a holder with the issuer.
    /// POST /v1alpha/participants/{participantContextId}/holders
    /// </summary>
    Task RegisterHolder(RegisterHolderRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Triggers a DCP CredentialOffer message being sent to the holder.
    /// POST /v1alpha/participants/{participantContextId}/credentials/offer
    /// </summary>
    Task TriggerCredentialOffer(TriggerCredentialOfferRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Revokes a credential.
    /// POST /v1alpha/participants/{participantContextId}/credentials/{credentialId}/revoke
    /// </summary>
    Task RevokeCredential(string credentialId, CancellationToken cancellationToken);

    /// <summary>
    /// Suspends a credential (reversible revocation).
    /// POST /v1alpha/participants/{participantContextId}/credentials/{credentialId}/suspend
    /// </summary>
    Task SuspendCredential(string credentialId, CancellationToken cancellationToken);

    /// <summary>
    /// Resumes a suspended credential.
    /// POST /v1alpha/participants/{participantContextId}/credentials/{credentialId}/resume
    /// </summary>
    Task ResumeCredential(string credentialId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the revocation status of a credential.
    /// GET /v1alpha/participants/{participantContextId}/credentials/{credentialId}/status
    /// </summary>
    Task<CredentialStatusResponse> GetCredentialStatus(string credentialId, CancellationToken cancellationToken);
}
