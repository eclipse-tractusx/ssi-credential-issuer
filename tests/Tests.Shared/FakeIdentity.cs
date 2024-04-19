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

using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Tests.Shared;

public class FakeIdentity : IIdentityData
{
    public string IdentityId => "ac1cf001-7fbc-1f2f-817f-bce058020001";
    public string Bpnl => "BPNL00000003AYRE";
    public bool IsServiceAccount => false;
}
