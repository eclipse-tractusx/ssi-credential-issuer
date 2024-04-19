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
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.AuditEntities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Auditing;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Auditing.Extensions;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Auditing.Handler;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using System.Collections.Immutable;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Entities;

public class IssuerDbContext : DbContext
{
    private readonly IAuditHandler _auditHandler;

    protected IssuerDbContext()
    {
        throw new InvalidOperationException("IdentityService should never be null");
    }

    public IssuerDbContext(DbContextOptions<IssuerDbContext> options, IAuditHandler auditHandler)
        : base(options)
    {
        _auditHandler = auditHandler;
    }

    public virtual DbSet<AuditCompanySsiDetail20240228> AuditCompanySsiDetail20240228 { get; set; } = default!;
    public virtual DbSet<AuditDocument20240305> AuditDocument20240305 { get; set; } = default!;
    public virtual DbSet<AuditCompanySsiDetail20240419> AuditCompanySsiDetail20240419 { get; set; } = default!;
    public virtual DbSet<AuditDocument20240419> AuditDocument20240419 { get; set; } = default!;
    public virtual DbSet<CompanySsiDetail> CompanySsiDetails { get; set; } = default!;
    public virtual DbSet<CompanySsiDetailAssignedDocument> CompanySsiDetailAssignedDocuments { get; set; } = default!;
    public virtual DbSet<CompanySsiDetailStatus> CompanySsiDetailStatuses { get; set; } = default!;
    public virtual DbSet<CompanySsiProcessData> CompanySsiProcessData { get; set; } = default!;
    public virtual DbSet<Document> Documents { get; set; } = default!;
    public virtual DbSet<DocumentStatus> DocumentStatus { get; set; } = default!;
    public virtual DbSet<DocumentType> DocumentTypes { get; set; } = default!;
    public virtual DbSet<ExpiryCheckType> ExpiryCheckTypes { get; set; } = default!;
    public virtual DbSet<MediaType> MediaTypes { get; set; } = default!;
    public virtual DbSet<Process> Processes { get; set; } = default!;
    public virtual DbSet<ProcessStep> ProcessSteps { get; set; } = default!;
    public virtual DbSet<ProcessStepStatus> ProcessStepStatuses { get; set; } = default!;
    public virtual DbSet<ProcessStepType> ProcessStepTypes { get; set; } = default!;
    public virtual DbSet<ProcessType> ProcessTypes { get; set; } = default!;
    public virtual DbSet<UseCase> UseCases { get; set; } = default!;
    public virtual DbSet<VerifiedCredentialExternalType> VerifiedCredentialExternalTypes { get; set; } = default!;
    public virtual DbSet<VerifiedCredentialExternalTypeDetailVersion> VerifiedCredentialExternalTypeDetailVersions { get; set; } = default!;
    public virtual DbSet<VerifiedCredentialType> VerifiedCredentialTypes { get; set; } = default!;
    public virtual DbSet<VerifiedCredentialTypeAssignedExternalType> VerifiedCredentialTypeAssignedExternalTypes { get; set; } = default!;
    public virtual DbSet<VerifiedCredentialTypeAssignedKind> VerifiedCredentialTypeAssignedKinds { get; set; } = default!;
    public virtual DbSet<VerifiedCredentialTypeAssignedUseCase> VerifiedCredentialTypeAssignedUseCases { get; set; } = default!;
    public virtual DbSet<VerifiedCredentialTypeKind> VerifiedCredentialTypeKinds { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "en_US.utf8");
        modelBuilder.HasDefaultSchema("issuer");

        modelBuilder.Entity<CompanySsiDetail>(entity =>
        {
            entity.HasOne(c => c.Process)
                .WithMany(c => c.CompanySsiDetails)
                .HasForeignKey(t => t.ProcessId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(c => c.VerifiedCredentialType)
                .WithMany(c => c.CompanySsiDetails)
                .HasForeignKey(t => t.VerifiedCredentialTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(c => c.CompanySsiDetailStatus)
                .WithMany(c => c.CompanySsiDetails)
                .HasForeignKey(t => t.CompanySsiDetailStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(c => c.VerifiedCredentialExternalTypeDetailVersion)
                .WithMany(c => c.CompanySsiDetails)
                .HasForeignKey(t => t.VerifiedCredentialExternalTypeDetailVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasMany(t => t.Documents)
                .WithMany(o => o.CompanySsiDetails)
                .UsingEntity<CompanySsiDetailAssignedDocument>(
                    j => j
                        .HasOne(d => d.Document!)
                        .WithMany()
                        .HasForeignKey(d => d.DocumentId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j => j
                        .HasOne(d => d.CompanySsiDetail!)
                        .WithMany()
                        .HasForeignKey(d => d.CompanySsiDetailId)
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey(e => new { e.DocumentId, e.CompanySsiDetailId });
                    });

            entity.HasAuditV2Triggers<CompanySsiDetail, AuditCompanySsiDetail20240419>();
        });

        modelBuilder.Entity<CompanySsiDetailStatus>()
            .HasData(
                Enum.GetValues(typeof(CompanySsiDetailStatusId))
                    .Cast<CompanySsiDetailStatusId>()
                    .Select(e => new CompanySsiDetailStatus(e))
            );

        modelBuilder.Entity<CompanySsiProcessData>(e =>
        {
            e.HasKey(x => x.CompanySsiDetailId);

            e.HasOne(x => x.CompanySsiDetail)
                .WithOne(x => x.CompanySsiProcessData)
                .HasForeignKey<CompanySsiProcessData>(x => x.CompanySsiDetailId);

            e.HasOne(x => x.CredentialTypeKind)
                .WithMany(x => x.CompanySsiProcessData)
                .HasForeignKey(x => x.CredentialTypeKindId);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasAuditV2Triggers<Document, AuditDocument20240419>();
        });

        modelBuilder.Entity<DocumentStatus>()
            .HasData(
                Enum.GetValues(typeof(DocumentStatusId))
                    .Cast<DocumentStatusId>()
                    .Select(e => new DocumentStatus(e))
            );

        modelBuilder.Entity<DocumentType>()
            .HasData(
                Enum.GetValues(typeof(DocumentTypeId))
                    .Cast<DocumentTypeId>()
                    .Select(e => new DocumentType(e))
            );

        modelBuilder.Entity<ExpiryCheckType>()
            .HasData(
                Enum.GetValues(typeof(ExpiryCheckTypeId))
                    .Cast<ExpiryCheckTypeId>()
                    .Select(e => new ExpiryCheckType(e))
            );

        modelBuilder.Entity<MediaType>()
            .HasData(
                Enum.GetValues(typeof(MediaTypeId))
                    .Cast<MediaTypeId>()
                    .Select(e => new MediaType(e))
            );

        modelBuilder.Entity<Process>()
            .HasOne(d => d.ProcessType)
            .WithMany(p => p!.Processes)
            .HasForeignKey(d => d.ProcessTypeId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<ProcessStep>()
            .HasOne(d => d.Process)
            .WithMany(p => p!.ProcessSteps)
            .HasForeignKey(d => d.ProcessId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<ProcessType>()
            .HasData(
                Enum.GetValues(typeof(ProcessTypeId))
                    .Cast<ProcessTypeId>()
                    .Select(e => new ProcessType(e))
            );

        modelBuilder.Entity<ProcessStepStatus>()
            .HasData(
                Enum.GetValues(typeof(ProcessStepStatusId))
                    .Cast<ProcessStepStatusId>()
                    .Select(e => new ProcessStepStatus(e))
            );

        modelBuilder.Entity<ProcessStepType>()
            .HasData(
                Enum.GetValues(typeof(ProcessStepTypeId))
                    .Cast<ProcessStepTypeId>()
                    .Select(e => new ProcessStepType(e))
            );

        modelBuilder.Entity<UseCase>();

        modelBuilder.Entity<VerifiedCredentialExternalType>()
            .HasData(
                Enum.GetValues(typeof(VerifiedCredentialExternalTypeId))
                    .Cast<VerifiedCredentialExternalTypeId>()
                    .Select(e => new VerifiedCredentialExternalType(e))
            );

        modelBuilder.Entity<VerifiedCredentialExternalTypeDetailVersion>(entity =>
        {
            entity.HasOne(d => d.VerifiedCredentialExternalType)
                .WithMany(x => x.VerifiedCredentialExternalTypeDetailVersions)
                .HasForeignKey(d => d.VerifiedCredentialExternalTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasIndex(e => new { e.VerifiedCredentialExternalTypeId, e.Version })
                .IsUnique();
        });
        modelBuilder.Entity<VerifiedCredentialType>()
            .HasData(
                Enum.GetValues(typeof(VerifiedCredentialTypeId))
                    .Cast<VerifiedCredentialTypeId>()
                    .Select(e => new VerifiedCredentialType(e))
            );

        modelBuilder.Entity<VerifiedCredentialTypeAssignedExternalType>(entity =>
        {
            entity.HasKey(e => new { e.VerifiedCredentialTypeId, e.VerifiedCredentialExternalTypeId });

            entity.HasOne(d => d.VerifiedCredentialType)
                .WithOne(x => x.VerifiedCredentialTypeAssignedExternalType)
                .HasForeignKey<VerifiedCredentialTypeAssignedExternalType>(d => d.VerifiedCredentialTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.VerifiedCredentialExternalType)
                .WithMany(x => x.VerifiedCredentialTypeAssignedExternalTypes)
                .HasForeignKey(d => d.VerifiedCredentialExternalTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<VerifiedCredentialTypeAssignedKind>(entity =>
        {
            entity.HasKey(e => new { e.VerifiedCredentialTypeId, e.VerifiedCredentialTypeKindId });

            entity.HasOne(d => d.VerifiedCredentialTypeKind)
                .WithMany(x => x.VerifiedCredentialTypeAssignedKinds)
                .HasForeignKey(d => d.VerifiedCredentialTypeKindId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.VerifiedCredentialType)
                .WithOne(x => x.VerifiedCredentialTypeAssignedKind)
                .HasForeignKey<VerifiedCredentialTypeAssignedKind>(d => d.VerifiedCredentialTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasIndex(x => x.VerifiedCredentialTypeId)
                .IsUnique(false);
        });

        modelBuilder.Entity<VerifiedCredentialTypeAssignedUseCase>(entity =>
        {
            entity.HasKey(x => new { x.VerifiedCredentialTypeId, x.UseCaseId });

            entity.HasOne(c => c.VerifiedCredentialType)
                .WithOne(c => c.VerifiedCredentialTypeAssignedUseCase)
                .HasForeignKey<VerifiedCredentialTypeAssignedUseCase>(c => c.VerifiedCredentialTypeId);

            entity.HasOne(c => c.UseCase)
                .WithOne(c => c.VerifiedCredentialAssignedUseCase)
                .HasForeignKey<VerifiedCredentialTypeAssignedUseCase>(c => c.UseCaseId);
        });

        modelBuilder.Entity<VerifiedCredentialTypeKind>()
            .HasData(
                Enum.GetValues(typeof(VerifiedCredentialTypeKindId))
                    .Cast<VerifiedCredentialTypeKindId>()
                    .Select(e => new VerifiedCredentialTypeKind(e))
            );
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        EnhanceChangedEntries();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnhanceChangedEntries();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override int SaveChanges()
    {
        EnhanceChangedEntries();
        return base.SaveChanges();
    }

    private void EnhanceChangedEntries()
    {
        _auditHandler.HandleAuditForChangedEntries(
            ChangeTracker.Entries().Where(entry =>
                entry.State != EntityState.Unchanged && entry.State != EntityState.Detached &&
                entry.Entity is IAuditableV2).ToImmutableList(),
            ChangeTracker.Context);
    }
}
