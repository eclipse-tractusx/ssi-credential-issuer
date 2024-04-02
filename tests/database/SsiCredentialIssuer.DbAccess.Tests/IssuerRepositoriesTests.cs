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
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.DbAccess.Tests;

public class IssuerRepositoriesTests : IAssemblyFixture<TestDbFixture>
{
    private readonly IFixture _fixture;
    private readonly TestDbFixture _dbTestDbFixture;

    public IssuerRepositoriesTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetInstance

    [Fact]
    public async Task GetInstance_CompanySsiDetails_CreatesSuccessfully()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = sut.GetInstance<ICompanySsiDetailsRepository>();

        // Assert
        result.Should().BeOfType<CompanySsiDetailsRepository>();
    }

    [Fact]
    public async Task GetInstance_Credential_CreatesSuccessfully()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = sut.GetInstance<ICredentialRepository>();

        // Assert
        result.Should().BeOfType<CredentialRepository>();
    }

    [Fact]
    public async Task GetInstance_DocumentRepo_CreatesSuccessfully()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = sut.GetInstance<IDocumentRepository>();

        // Assert
        result.Should().BeOfType<DocumentRepository>();
    }

    [Fact]
    public async Task GetInstance_ProcessStep_CreatesSuccessfully()
    {
        // Arrange
        var sut = await CreateSut().ConfigureAwait(false);

        // Act
        var result = sut.GetInstance<IProcessStepRepository>();

        // Assert
        result.Should().BeOfType<ProcessStepRepository>();
    }

    #endregion

    #region Clear

    [Fact]
    public async Task Clear_CreateSuccessfully()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);
        var changeTracker = dbContext.ChangeTracker;
        dbContext.Processes.Add(new Process(Guid.NewGuid(), ProcessTypeId.CREATE_CREDENTIAL, Guid.NewGuid()));

        // Act
        sut.Clear();

        // Assert
        changeTracker.HasChanges().Should().BeFalse();
        changeTracker.Entries().Should().BeEmpty();
    }

    #endregion

    #region Attach

    [Fact]
    public async Task Attach_CreateSuccessfully()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext().ConfigureAwait(false);
        var changeTracker = dbContext.ChangeTracker;
        var now = DateTimeOffset.Now;

        // Act
        sut.Attach(new Process(new Guid("dd371565-9489-4907-a2e4-b8cbfe7a8cd2"), default, Guid.Empty), p =>
        {
            p.LockExpiryDate = now;
            p.ProcessTypeId = ProcessTypeId.CREATE_CREDENTIAL;
        });

        // Assert
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should()
            .ContainSingle()
            .Which.State.Should().Be(EntityState.Modified);
        changeTracker.Entries().Select(x => x.Entity).Cast<Process>()
            .Should().Satisfy(x => x.ProcessTypeId == ProcessTypeId.CREATE_CREDENTIAL);
    }

    #endregion

    private async Task<(IssuerRepositories sut, IssuerDbContext dbContext)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetDbContext().ConfigureAwait(false);
        var sut = new IssuerRepositories(context);
        return (sut, context);
    }

    private async Task<IssuerRepositories> CreateSut()
    {
        var context = await _dbTestDbFixture.GetDbContext().ConfigureAwait(false);
        var sut = new IssuerRepositories(context);
        return sut;
    }
}
