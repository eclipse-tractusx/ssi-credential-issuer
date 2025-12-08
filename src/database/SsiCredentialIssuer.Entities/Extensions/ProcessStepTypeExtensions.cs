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

using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Extensions;

public static class ProcessStepTypeExtensions
{
    public static ProcessStepTypeId GetRetriggerStep(this ProcessStepTypeId processStepTypeId) =>
        processStepTypeId switch
        {
            ProcessStepTypeId.CREATE_SIGNED_CREDENTIAL => ProcessStepTypeId.RETRIGGER_CREATE_SIGNED_CREDENTIAL,
            ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT => ProcessStepTypeId.RETRIGGER_SAVE_CREDENTIAL_DOCUMENT,
            ProcessStepTypeId.CREATE_CREDENTIAL_FOR_HOLDER => ProcessStepTypeId.RETRIGGER_CREATE_CREDENTIAL_FOR_HOLDER,
            ProcessStepTypeId.REQUEST_CREDENTIAL_FOR_HOLDER => ProcessStepTypeId.RETRIGGER_REQUEST_CREDENTIAL_FOR_HOLDER,
            ProcessStepTypeId.REQUEST_CREDENTIAL_AUTO_APPROVE => ProcessStepTypeId.RETRIGGER_REQUEST_CREDENTIAL_AUTO_APPROVE,
            ProcessStepTypeId.REQUEST_CREDENTIAL_STATUS_CHECK => ProcessStepTypeId.RETRIGGER_REQUEST_CREDENTIAL_STATUS_CHECK,
            ProcessStepTypeId.TRIGGER_CALLBACK => ProcessStepTypeId.RETRIGGER_TRIGGER_CALLBACK,
            ProcessStepTypeId.REVOKE_CREDENTIAL => ProcessStepTypeId.RETRIGGER_REVOKE_CREDENTIAL,
            ProcessStepTypeId.TRIGGER_NOTIFICATION => ProcessStepTypeId.RETRIGGER_TRIGGER_NOTIFICATION,
            ProcessStepTypeId.TRIGGER_MAIL => ProcessStepTypeId.RETRIGGER_TRIGGER_MAIL,
            _ => throw new ArgumentOutOfRangeException($"{processStepTypeId} is not a valid value")
        };

    public static (ProcessTypeId processTypeId, ProcessStepTypeId processStepTypeId) GetProcessStepForRetrigger(this ProcessStepTypeId processStepTypeId) =>
        processStepTypeId switch
        {
            ProcessStepTypeId.RETRIGGER_CREATE_SIGNED_CREDENTIAL => (ProcessTypeId.CREATE_CREDENTIAL, ProcessStepTypeId.CREATE_SIGNED_CREDENTIAL),
            ProcessStepTypeId.RETRIGGER_SAVE_CREDENTIAL_DOCUMENT => (ProcessTypeId.CREATE_CREDENTIAL, ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT),
            ProcessStepTypeId.RETRIGGER_CREATE_CREDENTIAL_FOR_HOLDER => (ProcessTypeId.CREATE_CREDENTIAL, ProcessStepTypeId.CREATE_CREDENTIAL_FOR_HOLDER),
            ProcessStepTypeId.RETRIGGER_REQUEST_CREDENTIAL_FOR_HOLDER => (ProcessTypeId.CREATE_CREDENTIAL, ProcessStepTypeId.REQUEST_CREDENTIAL_FOR_HOLDER),
            ProcessStepTypeId.RETRIGGER_REQUEST_CREDENTIAL_AUTO_APPROVE => (ProcessTypeId.CREATE_CREDENTIAL, ProcessStepTypeId.REQUEST_CREDENTIAL_AUTO_APPROVE),
            ProcessStepTypeId.RETRIGGER_REQUEST_CREDENTIAL_STATUS_CHECK => (ProcessTypeId.CREATE_CREDENTIAL, ProcessStepTypeId.REQUEST_CREDENTIAL_STATUS_CHECK),
            ProcessStepTypeId.RETRIGGER_TRIGGER_CALLBACK => (ProcessTypeId.CREATE_CREDENTIAL, ProcessStepTypeId.TRIGGER_CALLBACK),
            ProcessStepTypeId.RETRIGGER_REVOKE_CREDENTIAL => (ProcessTypeId.DECLINE_CREDENTIAL, ProcessStepTypeId.REVOKE_CREDENTIAL),
            ProcessStepTypeId.RETRIGGER_TRIGGER_NOTIFICATION => (ProcessTypeId.DECLINE_CREDENTIAL, ProcessStepTypeId.TRIGGER_NOTIFICATION),
            ProcessStepTypeId.RETRIGGER_TRIGGER_MAIL => (ProcessTypeId.DECLINE_CREDENTIAL, ProcessStepTypeId.TRIGGER_MAIL),
            _ => throw new ArgumentOutOfRangeException($"{processStepTypeId} is not a valid value")
        };
}
