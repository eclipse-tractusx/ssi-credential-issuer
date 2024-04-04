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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DbAccess.Tests.Setup;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.AuditEntities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Auditing.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.DbAccess.Tests;

public class IssuerDbContextTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly IDateTimeProvider _dateTimeProvider;

#pragma warning disable xUnit1041
    public IssuerDbContextTests(TestDbFixture testDbFixture)
#pragma warning restore xUnit1041
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
        _dateTimeProvider = A.Fake<IDateTimeProvider>();
    }

    #region SaveAuditableEntity

    [Fact]
    public async Task SaveCreatedAuditableEntity_SetsLastEditorId()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);

        var before = now.AddDays(-1);
        var id = Guid.NewGuid();
        var ca = new CompanySsiDetail(id, "BPNL00000001TEST", VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER, CompanySsiDetailStatusId.ACTIVE, "BPNL0001ISSUER", new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), before);

        var sut = await CreateContext();
        using var trans = await sut.Database.BeginTransactionAsync();

        // Act
        sut.Add(ca);
        await sut.SaveChangesAsync();

        // Assert
        ca.LastEditorId.Should().NotBeNull().And.Be(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"));
        ca.DateLastChanged.Should().Be(now);
        var auditEntries = await sut.AuditCompanySsiDetail20240228.Where(x => x.Id == id).ToListAsync();
        auditEntries.Should().ContainSingle().Which.Should().Match<AuditCompanySsiDetail20240228>(
            x => x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.ACTIVE && (x.DateCreated - before) < TimeSpan.FromSeconds(1) && x.AuditV1OperationId == AuditOperationId.INSERT && (x.AuditV1DateLastChanged - now) < TimeSpan.FromSeconds(1) && x.LastEditorId == new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"));
        await trans.RollbackAsync();
    }

    [Fact]
    public async Task SaveDeletedAuditableEntity_SetsLastEditorId()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var later = now.AddMinutes(1);
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now).Once().Then.Returns(later);

        var before = now.AddDays(-1);
        var id = Guid.NewGuid();
        var ca = new CompanySsiDetail(id, "BPNL00000001TEST", VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER, CompanySsiDetailStatusId.ACTIVE, "BPNL0001ISSUER", new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"), before);

        var sut = await CreateContext();
        using var trans = await sut.Database.BeginTransactionAsync();

        // Act
        sut.Add(ca);
        await sut.SaveChangesAsync();
        sut.Remove(ca);
        await sut.SaveChangesAsync();

        // Assert
        ca.LastEditorId.Should().NotBeNull().And.Be(new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"));
        ca.DateLastChanged.Should().Be(later);
        var auditEntries = await sut.AuditCompanySsiDetail20240228.Where(x => x.Id == id).ToListAsync();
        auditEntries.Should().HaveCount(2).And.Satisfy(
            x => x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.ACTIVE && (x.DateCreated - before) < TimeSpan.FromSeconds(1) && x.AuditV1OperationId == AuditOperationId.INSERT && x.LastEditorId == new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"),
            x => x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.ACTIVE && (x.DateCreated - before) < TimeSpan.FromSeconds(1) && x.AuditV1OperationId == AuditOperationId.DELETE && (x.AuditV1DateLastChanged - later) < TimeSpan.FromSeconds(1) && x.LastEditorId == new Guid("ac1cf001-7fbc-1f2f-817f-bce058020001"));
        await trans.RollbackAsync();
    }

    #endregion

    private async Task<IssuerDbContext> CreateContext() =>
        await _dbTestDbFixture.GetDbContext(_dateTimeProvider);
}
