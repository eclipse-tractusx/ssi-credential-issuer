/********************************************************************************
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;

public enum ProcessStepTypeId
{
    // CREATE CREDENTIAL PROCESS
    CREATE_SIGNED_CREDENTIAL = 1,
    SAVE_CREDENTIAL_DOCUMENT = 3,
    CREATE_CREDENTIAL_FOR_HOLDER = 4,
    REQUEST_CREDENTIAL_FOR_HOLDER = 10,
    REQUEST_CREDENTIAL_STATUS_CHECK = 12,
    REQUEST_CREDENTIAL_AUTO_APPROVE = 14,
    TRIGGER_CALLBACK = 5,
    RETRIGGER_CREATE_SIGNED_CREDENTIAL = 6,
    RETRIGGER_SAVE_CREDENTIAL_DOCUMENT = 7,
    RETRIGGER_CREATE_CREDENTIAL_FOR_HOLDER = 8,
    RETRIGGER_REQUEST_CREDENTIAL_FOR_HOLDER = 11,
    RETRIGGER_REQUEST_CREDENTIAL_STATUS_CHECK = 13,
    RETRIGGER_REQUEST_CREDENTIAL_AUTO_APPROVE = 15,
    RETRIGGER_TRIGGER_CALLBACK = 9,

    // DECLINE PROCESS
    REVOKE_CREDENTIAL = 100,
    TRIGGER_NOTIFICATION = 101,
    TRIGGER_MAIL = 102,
    RETRIGGER_REVOKE_CREDENTIAL = 103,
    RETRIGGER_TRIGGER_NOTIFICATION = 104,
    RETRIGGER_TRIGGER_MAIL = 105
}
