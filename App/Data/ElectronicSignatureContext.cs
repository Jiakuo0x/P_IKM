namespace Data;

public class ElectronicSignatureContext : DbContextBase
{
    public ElectronicSignatureContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Models.TemplateMapping>? TemplateMappings { get; set; }
    public DbSet<Models.ElectronicSignatureTask>? Tasks { get; set; }
    public DbSet<Models.ElectronicSignatureTaskLog>? TaskLogs { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Models.ElectronicSignatureTask>()
            .HasIndex(i => i.DocuSignEnvelopeId);

        modelBuilder.Entity<Models.ElectronicSignatureTaskLog>()
            .HasIndex(i => i.TaskId);

        modelBuilder.Entity<Models.TemplateMapping>()
            .HasIndex(i => i.DocuSignTemplateId);
    }
}