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
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DbAccess.Tests.Setup;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using System.Text.Json;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.DbAccess.Tests;

public class CompanySsiDetailsRepositoryTests
{
    private const string ValidBpnl = "BPNL00000003AYRE";
    private readonly TestDbFixture _dbTestDbFixture;
    private readonly string _userId = "ac1cf001-7fbc-1f2f-817f-bce058020006";

    public CompanySsiDetailsRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetDetailsForCompany

    [Fact]
    public async Task GetDetailsForCompany_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetUseCaseParticipationForCompany(ValidBpnl, DateTimeOffset.MinValue).ToListAsync();

        // Assert
        result.Should().HaveCount(10);
        result.Where(x => x.Description != null).Should().HaveCount(8).And.Satisfy(
            x => x.Description == "T",
            x => x.Description == "BT",
            x => x.Description == "CE",
            x => x.Description == "QM",
            x => x.Description == "DCM",
            x => x.Description == "Puris",
            x => x.Description == "BPDM",
            x => x.Description == "DEG");
        var traceability = result.Single(x => x.CredentialType == VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK);
        traceability.VerifiedCredentials.Should().HaveCount(3).And.Satisfy(
            x => x.ExternalDetailData.Version == "1.0" && x.SsiDetailData.Single().ParticipationStatus == CompanySsiDetailStatusId.PENDING,
            x => x.ExternalDetailData.Version == "2.0" && !x.SsiDetailData.Any(),
            x => x.ExternalDetailData.Version == "3.0" && !x.SsiDetailData.Any());
    }

    [Fact]
    public async Task GetDetailsForCompany_WithExpiryFilter_ReturnsExpected()
    {
        // Arrange
        var dt = new DateTimeOffset(2023, 9, 29, 0, 0, 0, TimeSpan.Zero);
        var sut = await CreateSut();

        // Act
        var result = await sut.GetUseCaseParticipationForCompany(ValidBpnl, dt).ToListAsync();

        // Assert
        result.Should().HaveCount(10);
        result.Where(x => x.Description != null).Should().HaveCount(8).And.Satisfy(
            x => x.Description == "T",
            x => x.Description == "BT",
            x => x.Description == "CE",
            x => x.Description == "QM",
            x => x.Description == "DCM",
            x => x.Description == "Puris",
            x => x.Description == "BPDM",
            x => x.Description == "DEG");
        var traceability = result.Single(x => x.CredentialType == VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK);
        traceability.VerifiedCredentials.Should().HaveCount(3).And.Satisfy(
            x => x.ExternalDetailData.Version == "1.0" && x.SsiDetailData.Count() == 1,
            x => x.ExternalDetailData.Version == "2.0" && !x.SsiDetailData.Any(),
            x => x.ExternalDetailData.Version == "3.0" && !x.SsiDetailData.Any());
    }

    #endregion

    #region GetAllCredentialDetails

    [Fact]
    public async Task GetAllCredentialDetails_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetAllCredentialDetails(null, null, null).ToListAsync();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(7);
        result.Should().HaveCount(7);
        result.Where(x => x.Bpnl == ValidBpnl).Should().HaveCount(6)
            .And.Satisfy(
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK && x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.PENDING,
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.PCF_FRAMEWORK && x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.PENDING,
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.MEMBERSHIP && x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.PENDING,
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.MEMBERSHIP && x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.INACTIVE,
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.MEMBERSHIP && x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.INACTIVE,
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.BEHAVIOR_TWIN_FRAMEWORK && x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.INACTIVE);
        result.Where(x => x.Bpnl == "BPNL00000001LLHA").Should().ContainSingle()
            .And.Satisfy(x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK);
    }

    [Fact]
    public async Task GetAllCredentialDetails_WithWithStatusId_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetAllCredentialDetails(CompanySsiDetailStatusId.PENDING, null, null).ToListAsync();

        // Assert
        result.Should().NotBeNull().And.HaveCount(4);
        result.Count.Should().Be(4);
        result.Where(x => x.Bpnl == ValidBpnl).Should().HaveCount(3)
            .And.Satisfy(
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK,
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.PCF_FRAMEWORK,
                x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.MEMBERSHIP);
        result.Should().ContainSingle(x => x.Bpnl == "BPNL00000001LLHA")
            .Which.Should().Match<CompanySsiDetail>(x => x.VerifiedCredentialTypeId == VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK);
    }

    [Fact]
    public async Task GetAllCredentialDetails_WithWithCredentialType_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetAllCredentialDetails(null, VerifiedCredentialTypeId.PCF_FRAMEWORK, null).ToListAsync();

        // Assert
        result.Should().NotBeNull().And.ContainSingle().Which.Bpnl.Should().Be(ValidBpnl);
        result.Count.Should().Be(1);
    }

    #endregion

    #region GetSsiCertificates

    [Fact]
    public async Task GetSsiCertificates_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetSsiCertificates(ValidBpnl, new DateTimeOffset(2023, 01, 01, 01, 01, 01, TimeSpan.Zero)).ToListAsync();

        // Assert
        result.Should().HaveCount(2)
            .And.Satisfy(
                x => x.CredentialType == VerifiedCredentialTypeId.MEMBERSHIP &&
                     x.Credentials.Count() == 1 &&
                     x.Credentials.Single().SsiDetailData.Count(ssi => ssi.ParticipationStatus == CompanySsiDetailStatusId.PENDING) == 1,
                x => x.CredentialType == VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER && !x.Credentials.Any()
                );
    }

    #endregion

    #region GetOwnCredentialDetails

    [Fact]
    public async Task GetOwnCredentialDetails_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetOwnCredentialDetails(ValidBpnl).ToListAsync();

        // Assert
        result.Should().HaveCount(6)
            .And.Satisfy(
                x => x.CredentialType == VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK && x.Status == CompanySsiDetailStatusId.PENDING,
                x => x.CredentialType == VerifiedCredentialTypeId.PCF_FRAMEWORK && x.Status == CompanySsiDetailStatusId.PENDING,
                x => x.CredentialType == VerifiedCredentialTypeId.MEMBERSHIP && x.Status == CompanySsiDetailStatusId.PENDING,
                x => x.CredentialType == VerifiedCredentialTypeId.BEHAVIOR_TWIN_FRAMEWORK && x.Status == CompanySsiDetailStatusId.INACTIVE,
                x => x.CredentialType == VerifiedCredentialTypeId.MEMBERSHIP && x.Status == CompanySsiDetailStatusId.INACTIVE,
                x => x.CredentialType == VerifiedCredentialTypeId.MEMBERSHIP && x.Status == CompanySsiDetailStatusId.INACTIVE
            );
    }

    [Fact]
    public async Task GetOwnCredentialDetails_WithBpnWithoutCredential_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetOwnCredentialDetails("BPNL000000INVALID").ToListAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateSsiDetails

    [Fact]
    public async Task CreateSsiDetails_WithValidData_ReturnsExpected()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.CreateSsiDetails(new("9f5b9934-4014-4099-91e9-7b1aee696b03"), VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, CompanySsiDetailStatusId.PENDING, "BPNL0001ISSUER", _userId, null);

        // Assert
        context.ChangeTracker.HasChanges().Should().BeTrue();
        context.ChangeTracker.Entries().Should().ContainSingle()
            .Which.Entity.Should().BeOfType<CompanySsiDetail>()
            .Which.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.PENDING);
    }

    #endregion

    #region CheckSsiDetailsExistsForCompany

    [Fact]
    public async Task CheckCredentialDetailsExistsForCompany_WithExistingData_ReturnsTrue()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckSsiDetailsExistsForCompany(ValidBpnl, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, VerifiedCredentialTypeKindId.FRAMEWORK, new Guid("1268a76a-ca19-4dd8-b932-01f24071d560"));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckCredentialDetailsExistsForCompany_WithNotExistingData_ReturnsTrue()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckSsiDetailsExistsForCompany("BPNL000000001TEST", VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, VerifiedCredentialTypeKindId.FRAMEWORK, new Guid("1268a76a-ca19-4dd8-b932-01f24071d560"));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckCredentialDetailsExistsForCompany_WithWrongTypeKindId_ReturnsTrue()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckSsiDetailsExistsForCompany("BPNL000000001TEST", VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, VerifiedCredentialTypeKindId.MEMBERSHIP, new Guid("1268a76a-ca19-4dd8-b932-01f24071d560"));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckCredentialDetailsExistsForCompany_WithInactive_ReturnsFalse()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckSsiDetailsExistsForCompany(ValidBpnl, VerifiedCredentialTypeId.BEHAVIOR_TWIN_FRAMEWORK, VerifiedCredentialTypeKindId.FRAMEWORK, new Guid("1268a76a-ca19-4dd8-b932-01f24071d562"));

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CheckUseCaseCredentialAndExternalTypeDetails

    [Theory]
    [InlineData(VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, "1268a76a-ca19-4dd8-b932-01f24071d560", "2024-10-16 +0")]
    [InlineData(VerifiedCredentialTypeId.PCF_FRAMEWORK, "1268a76a-ca19-4dd8-b932-01f24071d561", "2024-10-16 +0")]
#pragma warning disable xUnit1012
    [InlineData(default, "1268a76a-ca19-6666-b932-01f24071d561", default)]
#pragma warning restore xUnit1012
    public async Task CheckUseCaseCredentialAndExternalTypeDetails_WithTypeId_ReturnsTrue(VerifiedCredentialTypeId typeId, Guid detailId, DateTimeOffset expiry)
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(detailId, typeId, ValidBpnl);

        // Assert
        result.Expiry.Should().Be(expiry);
    }

    #endregion

    #region CheckSsiCertificateType

    [Theory]
    [InlineData(VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, false)]
    [InlineData(VerifiedCredentialTypeId.PCF_FRAMEWORK, false)]
    [InlineData(VerifiedCredentialTypeId.BEHAVIOR_TWIN_FRAMEWORK, false)]
    [InlineData(VerifiedCredentialTypeId.MEMBERSHIP, true)]
    public async Task CheckSsiCertificateType_WithTypeId_ReturnsTrue(VerifiedCredentialTypeId typeId, bool expectedResult)
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.CheckSsiCertificateType(typeId);

        // Assert
        result.Exists.Should().Be(expectedResult);
    }

    #endregion

    #region GetSsiApprovalData

    [Fact]
    public async Task GetSsiApprovalData_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetSsiApprovalData(new("9f5b9934-4014-4099-91e9-7b1aee696b03"));

        // Assert
        result.exists.Should().BeTrue();
        result.data.Bpn.Should().Be("BPNL00000003AYRE");
        result.data.DetailData.Should().NotBeNull();
        result.data.DetailData!.VerifiedCredentialExternalTypeId.Should().Be(VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL);
    }

    [Fact]
    public async Task GetSsiApprovalData_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetSsiApprovalData(Guid.NewGuid());

        // Assert
        result.exists.Should().BeFalse();
    }

    #endregion

    #region GetAllCredentialDetails

    [Fact]
    public async Task GetSsiRejectionData_WithValidData_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetSsiRejectionData(new("9f5b9934-4014-4099-91e9-7b1aee696b03"));

        // Assert
        result.Exists.Should().BeTrue();
        result.Status.Should().Be(CompanySsiDetailStatusId.PENDING);
        result.Type.Should().Be(VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL);
    }

    [Fact]
    public async Task GetSsiRejectionData_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetSsiRejectionData(Guid.NewGuid());

        // Assert
        result.Should().Be(default);
    }

    #endregion

    #region AttachAndModifyCompanySsiDetails

    [Fact]
    public async Task AttachAndModifyCompanySsiDetails_WithValidData_ReturnsExpected()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyCompanySsiDetails(new("9f5b9934-4014-4099-91e9-7b1aee696b03"), null, ssi =>
            {
                ssi.CompanySsiDetailStatusId = CompanySsiDetailStatusId.INACTIVE;
            });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle()
            .Which.Entity.Should().BeOfType<CompanySsiDetail>()
            .And.Match<CompanySsiDetail>(x => x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.INACTIVE);
    }

    [Fact]
    public async Task AttachAndModifyCompanySsiDetails_WithNoChanges_ReturnsExpected()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyCompanySsiDetails(new("9f5b9934-4014-4099-91e9-7b1aee696b03"), ssi =>
        {
            ssi.CompanySsiDetailStatusId = CompanySsiDetailStatusId.INACTIVE;
        }, ssi =>
        {
            ssi.CompanySsiDetailStatusId = CompanySsiDetailStatusId.INACTIVE;
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeFalse();
        changedEntries.Should().ContainSingle()
            .Which.Entity.Should().BeOfType<CompanySsiDetail>()
            .And.Match<CompanySsiDetail>(x => x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.INACTIVE);
    }

    #endregion

    #region GetCertificateTypes

    [Fact]
    public async Task GetCertificateTypes_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetCertificateTypes(ValidBpnl).ToListAsync();

        // Assert
        result.Should().ContainSingle().Which.Should().Be(VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER);
    }

    [Fact]
    public async Task GetCertificateTypes_WithoutCertificate_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetCertificateTypes("BPNL0000001TEST").ToListAsync();

        // Assert
        result.Should().HaveCount(2).And.Satisfy(
            x => x == VerifiedCredentialTypeId.MEMBERSHIP,
                 x => x == VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER);
    }

    #endregion

    #region GetExpiryData

    [Fact]
    public async Task GetExpiryData_ReturnsExpected()
    {
        // Arrange
        var now = new DateTimeOffset(2025, 01, 1, 1, 1, 1, TimeSpan.Zero);
        var inactiveVcsToDelete = now.AddMonths(-12);
        var expiredVcsToDelete = now.AddDays(-42);
        var sut = await CreateSut();

        // Act
        var result = await sut.GetExpiryData(now, inactiveVcsToDelete, expiredVcsToDelete).ToListAsync();

        // Assert
        result.Should().HaveCount(6);
        result.Where(x => x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.PENDING).Should().HaveCount(3);
        result.Where(x => x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.INACTIVE).Should().HaveCount(3);
    }

    #endregion

    #region CreateCredentialDetails

    [Fact]
    public async Task CreateProcessData_WithValidData_ReturnsExpected()
    {
        // Arrange
        var json = JsonDocument.Parse("""
                           {
                            "root": "test123"
                           }
                           """);
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.CreateProcessData(Guid.NewGuid(), json, VerifiedCredentialTypeKindId.BPN, x => x.ClientId = "c1");

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle()
            .Which.Entity.Should().BeOfType<CompanySsiProcessData>()
            .And.Match<CompanySsiProcessData>(x => x.ClientId == "c1");
    }

    #endregion

    #region RemoveSsiDetail

    [Fact]
    public async Task RemoveSsiDetail_WithValidData_ReturnsExpected()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.RemoveSsiDetail(Guid.NewGuid(), ValidBpnl, "user1");

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle().Which.State.Should().Be(EntityState.Deleted);
    }

    #endregion

    #region AttachAndModifyProcessData

    [Fact]
    public async Task AttachAndModifyProcessData_WithValidData_ReturnsExpected()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyProcessData(new("9f5b9934-4014-4099-91e9-7b1aee696b03"), null, ssi =>
        {
            ssi.EncryptionMode = 1;
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle()
            .Which.Entity.Should().BeOfType<CompanySsiProcessData>()
            .And.Match<CompanySsiProcessData>(x => x.EncryptionMode == 1);
    }

    [Fact]
    public async Task AttachAndModifyProcessData_WithNoChanges_ReturnsExpected()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyProcessData(new("9f5b9934-4014-4099-91e9-7b1aee696b03"), ssi =>
        {
            ssi.EncryptionMode = 1;
        }, ssi =>
        {
            ssi.EncryptionMode = 1;
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeFalse();
        changedEntries.Should().ContainSingle()
            .Which.Entity.Should().BeOfType<CompanySsiProcessData>()
            .And.Match<CompanySsiProcessData>(x => x.EncryptionMode == 1);
    }

    #endregion

    #region Setup

    private async Task<CompanySsiDetailsRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetDbContext();
        return new CompanySsiDetailsRepository(context);
    }

    private async Task<(CompanySsiDetailsRepository sut, IssuerDbContext context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetDbContext();
        return (new CompanySsiDetailsRepository(context), context);
    }

    #endregion
}
