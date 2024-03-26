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
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.DbAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="CredentialRepositoryTests"/>
/// </summary>
public class CredentialRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public CredentialRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetDataForProcessId

    [Fact]
    public async Task GetDataForProcessId_ReturnsExpectedDocument()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetDataForProcessId(new Guid("dd371565-9489-4907-a2e4-b8cbfe7a8cd2"));

        // Assert
        result.Exists.Should().BeTrue();
        result.CredentialId.Should().Be(new Guid("9f5b9934-4014-4099-91e9-7b1aee696b03"));
    }

    #endregion

    #region GetCredentialData

    [Fact]
    public async Task GetCredentialData_ReturnsExpectedDocument()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetCredentialData(new Guid("9f5b9934-4014-4099-91e9-7b1aee696b03"));

        // Assert
        result.HolderWalletData.WalletUrl.Should().Be("https://example.org/wallet");
        result.HolderWalletData.ClientId.Should().Be("c123");
        result.EncryptionInformation.EncryptionMode.Should().Be(1);
    }

    #endregion

    #region GetWalletCredentialId

    [Fact]
    public async Task GetWalletCredentialId_ReturnsExpectedDocument()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetWalletCredentialId(new Guid("9f5b9934-4014-4099-91e9-7b1aee696b03"));

        // Assert
        result.Should().Be(new Guid("bd474c60-e7ce-450f-bdf4-73604546fc5e"));
    }

    #endregion

    #region GetCredentialStorageInformationById

    [Fact]
    public async Task GetCredentialStorageInformationById_ReturnsExpectedDocument()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetCredentialStorageInformationById(new Guid("9f5b9934-4014-4099-91e9-7b1aee696b03"));

        // Assert
        result.CredentialTypeKindId.Should().Be(VerifiedCredentialTypeKindId.FRAMEWORK);
        result.Schema.RootElement.GetRawText().Should().Be("{\"root\": \"test123\"}");
    }

    #endregion

    #region GetExternalCredentialAndKindId

    [Fact]
    public async Task GetExternalCredentialAndKindId_ReturnsExpectedDocument()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = await sut.GetExternalCredentialAndKindId(new Guid("9f5b9934-4014-4099-91e9-7b1aee696b03"));

        // Assert
        result.ExternalCredentialId.Should().Be(new Guid("bd474c60-e7ce-450f-bdf4-73604546fc5e"));
        result.KindId.Should().Be(VerifiedCredentialTypeKindId.FRAMEWORK);
    }

    #endregion

    #region AttachAndModifyCredential

    [Fact]
    public async Task AttachAndModifyCredential_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSutWithContext().ConfigureAwait(false);

        // Act
        sut.AttachAndModifyCredential(Guid.NewGuid(), x => x.CompanySsiDetailStatusId = CompanySsiDetailStatusId.ACTIVE, x => x.CompanySsiDetailStatusId = CompanySsiDetailStatusId.PENDING);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().ContainSingle();
        var entity = changedEntries.Single();
        entity.State.Should().Be(EntityState.Modified);
        ((CompanySsiDetail)entity.Entity).CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.PENDING);
    }

    #endregion

    #region Setup

    private async Task<CredentialRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetDbContext().ConfigureAwait(false);
        var sut = new CredentialRepository(context);
        return sut;
    }

    private async Task<(CredentialRepository Sut, IssuerDbContext Context)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetDbContext().ConfigureAwait(false);
        var sut = new CredentialRepository(context);
        return (sut, context);
    }

    #endregion
}
