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

using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;

public record CredentialExpiryData(
    Guid Id,
    string RequesterId,
    DateTimeOffset? ExpiryDate,
    ExpiryCheckTypeId? ExpiryCheckTypeId,
    string? DetailVersion,
    string Bpnl,
    CompanySsiDetailStatusId CompanySsiDetailStatusId,
    VerifiedCredentialExternalTypeId VerifiedCredentialExternalTypeId,
    CredentialScheduleData ScheduleData);

public record CredentialScheduleData(
    bool IsVcToDelete,
    bool IsOneDayNotification,
    bool IsTwoWeeksNotification,
    bool IsOneMonthNotification,
    bool IsVcToDecline
);

public record CredentialAboutToExpireData(
    Guid Id,
    string HolderBpn,
    VerifiedCredentialTypeId VerifiedCredentialTypeId,
    VerifiedCredentialTypeKindId VerifiedCredentialTypeKindId,
    JsonDocument Schema,
    string IdentityId,
    string? WalletUrl,
    Guid? DetailVersionId,
    string? CallbackUrl
);

