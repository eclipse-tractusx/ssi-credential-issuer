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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Auditing.Attributes;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Auditing.Enums;
using System.Collections.Immutable;
using System.Reflection;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Auditing.Extensions;

public static class AuditExtensions
{
    public record AuditPropertyInformation
    (
        Type AuditEntityType,
        IEnumerable<PropertyInfo> SourceProperties,
        IEnumerable<PropertyInfo> AuditProperties,
        IEnumerable<PropertyInfo> TargetProperties
    );

    public static AuditOperationId ToAuditOperation(this EntityState state) =>
        state switch
        {
            EntityState.Added => AuditOperationId.INSERT,
            EntityState.Deleted => AuditOperationId.DELETE,
            EntityState.Modified => AuditOperationId.UPDATE,
            _ => throw new ConflictException($"Entries with state {state} should not be audited")
        };

    public static AuditPropertyInformation? GetAuditV2PropertyInformation(this Type auditableEntityType)
    {
        var auditEntityAttribute =
            (AuditEntityV2Attribute?)Attribute.GetCustomAttribute(auditableEntityType, typeof(AuditEntityV2Attribute));
        if (auditEntityAttribute == null)
        {
            return null;
        }

        var auditEntityType = auditEntityAttribute.AuditEntityType;
        if (!typeof(IAuditEntityV2).IsAssignableFrom(auditEntityType))
        {
            throw new ConflictException($"{auditEntityType} must inherit from {nameof(IAuditEntityV2)}");
        }

        var sourceProperties = (typeof(IBaseEntity).IsAssignableFrom(auditableEntityType)
                ? typeof(IBaseEntity).GetProperties()
                : Enumerable.Empty<PropertyInfo>())
            .Concat(auditableEntityType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(p => !(p.GetGetMethod()?.IsVirtual ?? false)))
            .ToImmutableList();
        var auditProperties = typeof(IAuditEntityV2).GetProperties();
        var targetProperties = auditEntityType.GetProperties().ExceptBy(auditProperties.Select(x => x.Name), p => p.Name).ToImmutableList();

        targetProperties
            .ExceptBy(Enumerable.Repeat(nameof(IBaseEntity.Id), 1), p => p.Name)
            .Where(x => x.PropertyType == typeof(Nullable<>))
            .IfAny(notNullableProperties =>
                throw new ConfigurationException($"Properties {string.Join(",", notNullableProperties.Select(x => x.Name))} of type {auditEntityType.Name} are not nullable"));

        return new AuditPropertyInformation(
            auditEntityType,
            sourceProperties,
            auditProperties,
            targetProperties);
    }
}
