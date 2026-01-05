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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DbAccess.Tests.Setup;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using System.Collections.Immutable;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.DbAccess.Tests;

public class ProcessStepRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly IFixture _fixture;
    private readonly TestDbFixture _dbTestDbFixture;

    public ProcessStepRepositoryTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region CreateProcess

    [Fact]
    public async Task CreateProcess_CreatesSuccessfully()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext();
        var changeTracker = dbContext.ChangeTracker;

        // Act
        var result = sut.CreateProcess(ProcessTypeId.CREATE_CREDENTIAL);

        // Assert
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should().HaveCount(1)
            .And.AllSatisfy(x =>
            {
                x.State.Should().Be(EntityState.Added);
                x.Entity.Should().BeOfType<Process>();
            });
        changeTracker.Entries().Select(x => x.Entity).Cast<Process>()
            .Should().Satisfy(
                x => x.Id == result.Id && x.ProcessTypeId == ProcessTypeId.CREATE_CREDENTIAL
            );
    }

    #endregion

    #region CreateProcessStepRange

    [Fact]
    public async Task CreateProcessStepRange_CreateSuccessfully()
    {
        // Arrange
        var processId = Guid.NewGuid();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(3).ToImmutableArray();
        var (sut, dbContext) = await CreateSutWithContext();
        var changeTracker = dbContext.ChangeTracker;

        // Act
        var result = sut.CreateProcessStepRange(processStepTypeIds.Select(processStepTypeId => (processStepTypeId, ProcessStepStatusId.TODO, processId)));

        // Assert
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should()
            .HaveSameCount(processStepTypeIds)
            .And.AllSatisfy(x =>
            {
                x.State.Should().Be(EntityState.Added);
                x.Entity.Should().BeOfType<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>();
            });
        changeTracker.Entries().Select(x => x.Entity).Cast<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>()
            .Should().Satisfy(
                x => x.Id == result.ElementAt(0).Id && x.ProcessId == processId && x.ProcessStepTypeId == processStepTypeIds[0] && x.ProcessStepStatusId == ProcessStepStatusId.TODO,
                x => x.Id == result.ElementAt(1).Id && x.ProcessId == processId && x.ProcessStepTypeId == processStepTypeIds[1] && x.ProcessStepStatusId == ProcessStepStatusId.TODO,
                x => x.Id == result.ElementAt(2).Id && x.ProcessId == processId && x.ProcessStepTypeId == processStepTypeIds[2] && x.ProcessStepStatusId == ProcessStepStatusId.TODO
            );
    }

    #endregion

    #region AttachAndModifyProcessStep

    [Fact]
    public async Task AttachAndModifyProcessStep_WithExistingProcessStep_UpdatesStatus()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyProcessStep(new Guid("48f35f84-8d98-4fbd-ba80-8cbce5eeadb5"),
            existing =>
            {
                existing.ProcessStepStatusId = ProcessStepStatusId.TODO;
            },
            modify =>
            {
                modify.ProcessStepStatusId = ProcessStepStatusId.DONE;
            }
        );

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Modified);
        changedEntity.Entity.Should().BeOfType<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>().Which.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
    }

    #endregion

    #region AttachAndModifyProcessSteps

    [Fact]
    public async Task AttachAndModifyProcessSteps_UpdatesStatus()
    {
        // Arrange
        var stepData = _fixture.CreateMany<(Guid ProcessStepId, ProcessStep<Process, ProcessTypeId, ProcessStepTypeId> InitialStep, ProcessStep<Process, ProcessTypeId, ProcessStepTypeId> ModifiedStep)>(5).ToImmutableArray();

        var (sut, dbContext) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyProcessSteps(stepData.Select(data => new ValueTuple<Guid, Action<IProcessStep<ProcessStepTypeId>>?, Action<IProcessStep<ProcessStepTypeId>>>(
            data.ProcessStepId,
            step =>
                {
                    step.ProcessStepStatusId = data.InitialStep.ProcessStepStatusId;
                    step.DateLastChanged = data.InitialStep.DateLastChanged;
                    step.Message = data.InitialStep.Message;
                },
            step =>
                {
                    step.ProcessStepStatusId = data.ModifiedStep.ProcessStepStatusId;
                    step.DateLastChanged = data.ModifiedStep.DateLastChanged;
                    step.Message = data.ModifiedStep.Message;
                })));

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().HaveCount(5).And.AllSatisfy(entry => entry.State.Should().Be(EntityState.Modified));
        changedEntries.Select(entry => entry.Entity).Should().AllBeOfType<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>().Which.Should().Satisfy(
            step => step.Id == stepData[0].ProcessStepId && step.ProcessStepStatusId == stepData[0].ModifiedStep.ProcessStepStatusId && step.DateLastChanged == stepData[0].ModifiedStep.DateLastChanged && step.Message == stepData[0].ModifiedStep.Message,
            step => step.Id == stepData[1].ProcessStepId && step.ProcessStepStatusId == stepData[1].ModifiedStep.ProcessStepStatusId && step.DateLastChanged == stepData[1].ModifiedStep.DateLastChanged && step.Message == stepData[1].ModifiedStep.Message,
            step => step.Id == stepData[2].ProcessStepId && step.ProcessStepStatusId == stepData[2].ModifiedStep.ProcessStepStatusId && step.DateLastChanged == stepData[2].ModifiedStep.DateLastChanged && step.Message == stepData[2].ModifiedStep.Message,
            step => step.Id == stepData[3].ProcessStepId && step.ProcessStepStatusId == stepData[3].ModifiedStep.ProcessStepStatusId && step.DateLastChanged == stepData[3].ModifiedStep.DateLastChanged && step.Message == stepData[3].ModifiedStep.Message,
            step => step.Id == stepData[4].ProcessStepId && step.ProcessStepStatusId == stepData[4].ModifiedStep.ProcessStepStatusId && step.DateLastChanged == stepData[4].ModifiedStep.DateLastChanged && step.Message == stepData[4].ModifiedStep.Message
        );
    }

    [Fact]
    public async Task AttachAndModifyProcessSteps_WithUnmodifiedData_SkipsUpdateStatus()
    {
        // Arrange
        var stepData = _fixture.CreateMany<(Guid ProcessStepId, ProcessStep<Process, ProcessTypeId, ProcessStepTypeId> InitialStep)>(5).ToImmutableArray();

        var (sut, dbContext) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyProcessSteps(stepData.Select(data => new ValueTuple<Guid, Action<IProcessStep<ProcessStepTypeId>>?, Action<IProcessStep<ProcessStepTypeId>>>(
            data.ProcessStepId,
            step =>
                {
                    step.ProcessStepStatusId = data.InitialStep.ProcessStepStatusId;
                    step.DateLastChanged = data.InitialStep.DateLastChanged;
                    step.Message = data.InitialStep.Message;
                },
            step =>
                {
                    step.DateLastChanged = data.InitialStep.DateLastChanged;
                })));

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeFalse();
        changedEntries.Should().HaveCount(5).And.AllSatisfy(entry => entry.State.Should().Be(EntityState.Unchanged));
        changedEntries.Select(entry => entry.Entity).Should().AllBeOfType<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>().Which.Should().Satisfy(
            step => step.Id == stepData[0].ProcessStepId && step.ProcessStepStatusId == stepData[0].InitialStep.ProcessStepStatusId && step.DateLastChanged == stepData[0].InitialStep.DateLastChanged && step.Message == stepData[0].InitialStep.Message,
            step => step.Id == stepData[1].ProcessStepId && step.ProcessStepStatusId == stepData[1].InitialStep.ProcessStepStatusId && step.DateLastChanged == stepData[1].InitialStep.DateLastChanged && step.Message == stepData[1].InitialStep.Message,
            step => step.Id == stepData[2].ProcessStepId && step.ProcessStepStatusId == stepData[2].InitialStep.ProcessStepStatusId && step.DateLastChanged == stepData[2].InitialStep.DateLastChanged && step.Message == stepData[2].InitialStep.Message,
            step => step.Id == stepData[3].ProcessStepId && step.ProcessStepStatusId == stepData[3].InitialStep.ProcessStepStatusId && step.DateLastChanged == stepData[3].InitialStep.DateLastChanged && step.Message == stepData[3].InitialStep.Message,
            step => step.Id == stepData[4].ProcessStepId && step.ProcessStepStatusId == stepData[4].InitialStep.ProcessStepStatusId && step.DateLastChanged == stepData[4].InitialStep.DateLastChanged && step.Message == stepData[4].InitialStep.Message
        );
    }

    [Fact]
    public async Task AttachAndModifyProcessSteps_WithUnmodifiedData_UpdatesLastChanged()
    {
        // Arrange
        var stepData = _fixture.CreateMany<(Guid ProcessStepId, ProcessStep<Process, ProcessTypeId, ProcessStepTypeId> InitialStep)>(5).ToImmutableArray();

        var (sut, dbContext) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyProcessSteps(stepData.Select(data => new ValueTuple<Guid, Action<IProcessStep<ProcessStepTypeId>>?, Action<IProcessStep<ProcessStepTypeId>>>(
            data.ProcessStepId,
            step =>
                {
                    step.ProcessStepStatusId = data.InitialStep.ProcessStepStatusId;
                    step.DateLastChanged = data.InitialStep.DateLastChanged;
                    step.Message = data.InitialStep.Message;
                },
            step => { })));

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().HaveCount(5).And.AllSatisfy(entry => entry.State.Should().Be(EntityState.Modified));
        changedEntries.Select(entry => entry.Entity).Should().AllBeOfType<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>().Which.Should().Satisfy(
            step => step.Id == stepData[0].ProcessStepId && step.ProcessStepStatusId == stepData[0].InitialStep.ProcessStepStatusId && step.DateLastChanged != stepData[0].InitialStep.DateLastChanged && step.Message == stepData[0].InitialStep.Message,
            step => step.Id == stepData[1].ProcessStepId && step.ProcessStepStatusId == stepData[1].InitialStep.ProcessStepStatusId && step.DateLastChanged != stepData[1].InitialStep.DateLastChanged && step.Message == stepData[1].InitialStep.Message,
            step => step.Id == stepData[2].ProcessStepId && step.ProcessStepStatusId == stepData[2].InitialStep.ProcessStepStatusId && step.DateLastChanged != stepData[2].InitialStep.DateLastChanged && step.Message == stepData[2].InitialStep.Message,
            step => step.Id == stepData[3].ProcessStepId && step.ProcessStepStatusId == stepData[3].InitialStep.ProcessStepStatusId && step.DateLastChanged != stepData[3].InitialStep.DateLastChanged && step.Message == stepData[3].InitialStep.Message,
            step => step.Id == stepData[4].ProcessStepId && step.ProcessStepStatusId == stepData[4].InitialStep.ProcessStepStatusId && step.DateLastChanged != stepData[4].InitialStep.DateLastChanged && step.Message == stepData[4].InitialStep.Message
        );
    }

    #endregion

    #region GetActiveProcesses

    [Fact]
    public async Task GetActiveProcess_LockExpired_ReturnsExpected()
    {
        // Arrange
        var processTypeIds = new[] { ProcessTypeId.CREATE_CREDENTIAL };
        var processStepTypeIds = new[] {
            ProcessStepTypeId.CREATE_SIGNED_CREDENTIAL,
            ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT,
            ProcessStepTypeId.OFFER_CREDENTIAL_TO_HOLDER,
            ProcessStepTypeId.REVOKE_OLD_CREDENTIAL,
            ProcessStepTypeId.TRIGGER_CALLBACK
        };

        var sut = await CreateSut();

        // Act
        var result = await sut.GetActiveProcesses(processTypeIds, processStepTypeIds, DateTimeOffset.Parse("2023-03-02 00:00:00.000000 +00:00")).ToListAsync();
        result.Should().HaveCount(1)
            .And.Satisfy(
                x => x.Id == new Guid("dd371565-9489-4907-a2e4-b8cbfe7a8cd2") && x.ProcessTypeId == ProcessTypeId.CREATE_CREDENTIAL && x.LockExpiryDate == DateTimeOffset.Parse("2023-03-01 00:00:00.000000 +00:00")
            );
    }

    [Fact]
    public async Task GetActiveProcess_Locked_ReturnsExpected()
    {
        // Arrange
        var processTypeIds = new[] { ProcessTypeId.CREATE_CREDENTIAL };
        var processStepTypeIds = new[] {
            ProcessStepTypeId.CREATE_SIGNED_CREDENTIAL,
            ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT,
            ProcessStepTypeId.OFFER_CREDENTIAL_TO_HOLDER,
            ProcessStepTypeId.REVOKE_OLD_CREDENTIAL,
            ProcessStepTypeId.TRIGGER_CALLBACK
        };

        var sut = await CreateSut();

        // Act
        var result = await sut.GetActiveProcesses(processTypeIds, processStepTypeIds, DateTimeOffset.Parse("2023-02-28 00:00:00.000000 +00:00")).ToListAsync();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetProcessStepData

    [Fact]
    public async Task GetProcessStepData_ReturnsExpected()
    {
        // Arrange
        var processId = new Guid("dd371565-9489-4907-a2e4-b8cbfe7a8cd2");
        var sut = await CreateSut();

        // Act
        var result = await sut.GetProcessStepData(processId).ToListAsync();
        result.Should().HaveCount(1)
            .And.Satisfy(
                x => x.ProcessStepId == new Guid("cd231cb8-55de-4ae4-b93f-d440512341fb") && x.ProcessStepTypeId == ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT
            );
    }

    #endregion

    #region IsValidProcess

    [Fact]
    public async Task IsValidProcess_WithTodoProcessStep_ReturnsExpected()
    {
        // Arrange
        var processId = new Guid("dd371565-9489-4907-a2e4-b8cbfe7a8cd2");
        var sut = await CreateSut();

        // Act
        var result = await sut.IsValidProcess(processId, ProcessTypeId.CREATE_CREDENTIAL, Enumerable.Repeat(ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT, 1));
        result.ProcessExists.Should().BeTrue();
        result.ProcessData.ProcessSteps.Should().ContainSingle()
            .And.Satisfy(
                x => x.ProcessStepTypeId == ProcessStepTypeId.SAVE_CREDENTIAL_DOCUMENT && x.ProcessStepStatusId == ProcessStepStatusId.TODO
            );
    }

    [Fact]
    public async Task IsValidProcess_WithDoneProcessStep_ReturnsExpected()
    {
        // Arrange
        var processId = new Guid("dd371565-9489-4907-a2e4-b8cbfe7a8cd2");
        var sut = await CreateSut();

        // Act
        var result = await sut.IsValidProcess(processId, ProcessTypeId.CREATE_CREDENTIAL, Enumerable.Repeat(ProcessStepTypeId.CREATE_SIGNED_CREDENTIAL, 1));
        result.ProcessExists.Should().BeTrue();
        result.ProcessData.ProcessSteps.Should().BeEmpty();
    }

    [Fact]
    public async Task IsValidProcess_WithNotExistingProcess_ReturnsExpected()
    {
        // Arrange
        var processId = Guid.NewGuid();
        var sut = await CreateSut();

        // Act
        var result = await sut.IsValidProcess(processId, ProcessTypeId.CREATE_CREDENTIAL, Enumerable.Repeat(ProcessStepTypeId.CREATE_SIGNED_CREDENTIAL, 1));
        result.ProcessExists.Should().BeFalse();
    }

    #endregion

    private async Task<(ProcessStepRepository<Process, ProcessType<Process, ProcessTypeId>, ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>, ProcessStepType<Process, ProcessTypeId, ProcessStepTypeId>, ProcessTypeId, ProcessStepTypeId> sut, IssuerDbContext dbContext)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetDbContext();
        var sut = new ProcessStepRepository<Process, ProcessType<Process, ProcessTypeId>, ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>, ProcessStepType<Process, ProcessTypeId, ProcessStepTypeId>, ProcessTypeId, ProcessStepTypeId>(new IssuerProcessDbContextAccess(context));
        return (sut, context);
    }

    private async Task<ProcessStepRepository<Process, ProcessType<Process, ProcessTypeId>, ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>, ProcessStepType<Process, ProcessTypeId, ProcessStepTypeId>, ProcessTypeId, ProcessStepTypeId>> CreateSut()
    {
        var context = await _dbTestDbFixture.GetDbContext();
        var sut = new ProcessStepRepository<Process, ProcessType<Process, ProcessTypeId>, ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>, ProcessStepType<Process, ProcessTypeId, ProcessStepTypeId>, ProcessTypeId, ProcessStepTypeId>(new IssuerProcessDbContextAccess(context));
        return sut;
    }
}
