namespace Database;

using Models;

public class ElectronicSignatureContext : DbContextBase
{
    public ElectronicSignatureContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<TemplateMapping>? TemplateMappings { get; set; }
    public DbSet<ElectronicSignatureTask>? Tasks { get; set; }
    public DbSet<ElectronicSignatureTaskLog>? TaskLogs { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ElectronicSignatureTask>()
            .HasIndex(i => i.DocuSignEnvelopeId);
        modelBuilder.Entity<ElectronicSignatureTask>()
            .HasIndex(i => i.BestSignContractId);

        modelBuilder.Entity<ElectronicSignatureTaskLog>()
            .HasIndex(i => i.TaskId);

        modelBuilder.Entity<TemplateMapping>()
            .HasIndex(i => i.DocuSignTemplateId);
    }
}