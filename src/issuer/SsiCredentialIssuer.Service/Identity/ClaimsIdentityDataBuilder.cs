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

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;

public class ClaimsIdentityDataBuilder : IClaimsIdentityDataBuilder
{
    private string? _identityId;
    private Guid? _companyUserId;
    private string? _bpnl;
    private bool? _isServiceAccount;

    public string IdentityId { get => _identityId ?? throw new UnexpectedConditionException("userId should never be null here (endpoint must be annotated with an identity policy)"); }
    public Guid? CompanyUserId { get => _companyUserId; }
    public string Bpnl { get => _bpnl ?? throw new UnexpectedConditionException("bpnl should never be null here (endpoint must be annotated with an identity policy)"); }
    public bool IsServiceAccount { get => _isServiceAccount ?? throw new UnexpectedConditionException("isServiceAccount should never be null here (endpoint must be annotated with an isServiceACcount"); }

    public void AddIdentityId(string identityId)
    {
        _identityId = identityId;
    }

    public void AddCompanyUserId(Guid companyUserId)
    {
        _companyUserId = companyUserId;
    }

    public void AddBpnl(string bpnl)
    {
        _bpnl = bpnl;
    }

    public void AddIsServiceAccount(bool isServiceAccount)
    {
        _isServiceAccount = isServiceAccount;
    }

    public IClaimsIdentityDataBuilderStatus Status { get; set; } = IClaimsIdentityDataBuilderStatus.Initial;
}
