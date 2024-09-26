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
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using System.Text;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.DbAccess.Tests;

/// <summary>
/// Tests the functionality of the <see cref="DocumentRepositoryTests"/>
/// </summary>
public class DocumentRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

#pragma warning disable xUnit1041
    public DocumentRepositoryTests(TestDbFixture testDbFixture)
#pragma warning restore xUnit1041
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region Create Document

    [Fact]
    public async Task CreateDocument_ReturnsExpectedDocument()
    {
        // Arrange
        var (sut, context) = await CreateSut();
        var test = "This is just test content";
        var content = Encoding.UTF8.GetBytes(test);

        // Act
        var result = sut.CreateDocument("New Document", content, content, MediaTypeId.PDF, DocumentTypeId.CREDENTIAL, doc =>
        {
            doc.DocumentStatusId = DocumentStatusId.INACTIVE;
        });

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        result.DocumentTypeId.Should().Be(DocumentTypeId.CREDENTIAL);
        result.DocumentStatusId.Should().Be(DocumentStatusId.INACTIVE);
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Added);
    }

    #endregion

    #region AssignDocumentToCompanySsiDetails

    [Fact]
    public async Task AssignDocumentToCompanySsiDetails_ReturnsExpectedDocument()
    {
        // Arrange
        var (sut, context) = await CreateSut();
        var companySsiDetailId = Guid.NewGuid();
        var documentId = Guid.NewGuid();

        // Act
        sut.AssignDocumentToCompanySsiDetails(documentId, companySsiDetailId);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Added);
    }

    #endregion

    #region AttachAndModifyDocuments

    [Fact]
    public async Task AttachAndModifyDocuments_ReturnsExpectedResult()
    {
        // Arrange
        var (sut, context) = await CreateSut();

        var documentData = new (Guid DocumentId, Action<Document>?, Action<Document>)[] {
            (Guid.NewGuid(), null, document => document.DocumentStatusId = DocumentStatusId.INACTIVE),
            (Guid.NewGuid(), document => document.DocumentStatusId = DocumentStatusId.INACTIVE, document => document.DocumentStatusId = DocumentStatusId.INACTIVE),
            (Guid.NewGuid(), document => document.DocumentStatusId = DocumentStatusId.ACTIVE, document => document.DocumentStatusId = DocumentStatusId.INACTIVE),
        };

        // Act
        sut.AttachAndModifyDocuments(documentData);

        // Assert
        var changeTracker = context.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().HaveCount(3).And.AllSatisfy(x => x.Entity.Should().BeOfType<Document>()).And.Satisfy(
            x => x.State == EntityState.Modified && ((Document)x.Entity).Id == documentData[0].DocumentId && ((Document)x.Entity).DocumentStatusId == DocumentStatusId.INACTIVE,
            x => x.State == EntityState.Unchanged && ((Document)x.Entity).Id == documentData[1].DocumentId && ((Document)x.Entity).DocumentStatusId == DocumentStatusId.INACTIVE,
            x => x.State == EntityState.Modified && ((Document)x.Entity).Id == documentData[2].DocumentId && ((Document)x.Entity).DocumentStatusId == DocumentStatusId.INACTIVE
        );
    }

    #endregion

    #region Setup    

    private async Task<(DocumentRepository, IssuerDbContext)> CreateSut()
    {
        var context = await _dbTestDbFixture.GetDbContext();
        var sut = new DocumentRepository(context);
        return (sut, context);
    }

    #endregion
}
