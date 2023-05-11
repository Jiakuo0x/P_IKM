namespace Database;

using Models;

/// <summary>
/// The electronic signature context in the database
/// </summary>
public class ElectronicSignatureContext : DbContextBase
{
    public ElectronicSignatureContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Template mappings
    /// </summary>
    public DbSet<TemplateMapping>? TemplateMappings { get; set; }

    /// <summary>
    /// Electronic signature tasks
    /// </summary>
    public DbSet<ElectronicSignatureTask>? Tasks { get; set; }

    /// <summary>
    /// Electronic signature task logs
    /// </summary>
    public DbSet<ElectronicSignatureTaskLog>? TaskLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Create database index
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