using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ETMF.Models;

namespace ETMF.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Zone> Zones { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Artifact> Artifacts { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentVersion> DocumentVersions { get; set; }
        public DbSet<Metadata> Metadata { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        
        // 新增的工作流相关表
        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<WorkflowStep> WorkflowSteps { get; set; }
        public DbSet<DocumentWorkflow> DocumentWorkflows { get; set; }
        public DbSet<WorkflowHistory> WorkflowHistory { get; set; }
        
        // 新增的文档提醒表
        public DbSet<DocumentReminder> DocumentReminders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置Zone实体
            modelBuilder.Entity<Zone>()
                .HasMany(z => z.Sections)
                .WithOne(s => s.Zone)
                .HasForeignKey(s => s.ZoneId)
                .OnDelete(DeleteBehavior.Cascade);

            // 配置Section实体
            modelBuilder.Entity<Section>()
                .HasMany(s => s.Artifacts)
                .WithOne(a => a.Section)
                .HasForeignKey(a => a.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // 配置Artifact实体
            modelBuilder.Entity<Artifact>()
                .HasMany(a => a.Documents)
                .WithOne(d => d.Artifact)
                .HasForeignKey(d => d.ArtifactId)
                .OnDelete(DeleteBehavior.Cascade);

            // 配置Document实体
            modelBuilder.Entity<Document>()
                .HasMany(d => d.Versions)
                .WithOne(v => v.Document)
                .HasForeignKey(v => v.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Document>()
                .HasMany(d => d.Metadata)
                .WithOne(m => m.Document)
                .HasForeignKey(m => m.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // 配置Workflow实体
            modelBuilder.Entity<Workflow>()
                .HasMany(w => w.Steps)
                .WithOne(s => s.Workflow)
                .HasForeignKey(s => s.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            // 配置DocumentWorkflow实体
            modelBuilder.Entity<DocumentWorkflow>()
                .HasOne(dw => dw.Document)
                .WithMany()
                .HasForeignKey(dw => dw.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DocumentWorkflow>()
                .HasOne(dw => dw.Workflow)
                .WithMany()
                .HasForeignKey(dw => dw.WorkflowId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DocumentWorkflow>()
                .HasOne(dw => dw.CurrentStep)
                .WithMany()
                .HasForeignKey(dw => dw.CurrentStepId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DocumentWorkflow>()
                .HasMany(dw => dw.History)
                .WithOne(h => h.DocumentWorkflow)
                .HasForeignKey(h => h.DocumentWorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            // 配置工作流历史记录关系
            modelBuilder.Entity<WorkflowHistory>()
                .HasOne(wh => wh.User)
                .WithMany()
                .HasForeignKey(wh => wh.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // 配置文档提醒关系
            modelBuilder.Entity<DocumentReminder>()
                .HasOne(dr => dr.Document)
                .WithMany()
                .HasForeignKey(dr => dr.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 