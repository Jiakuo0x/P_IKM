using Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddDbContext<DbContext, Database.ElectronicSignatureContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("App")));

#region  DocuSign
builder.Services.AddOptions<Lib.DocuSign.Configuration>()
    .Bind(builder.Configuration.GetSection("DocuSign"));
builder.Services.AddScoped<Lib.DocuSign.ClientManager>();
builder.Services.AddScoped<DocuSignService>();
#endregion

#region  BestSign
builder.Services.AddOptions<Lib.BestSign.Configuration>()
    .Bind(builder.Configuration.GetSection("BestSign"));
builder.Services.AddScoped<Lib.BestSign.TokenManager>();
builder.Services.AddScoped<BestSignService>();
#endregion

#region  Services
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<TemplateMappingService>();
builder.Services.AddScoped<DocumentService>();
#endregion

#region  Azure Key Vault
builder.Services.AddOptions<Lib.Azure.KeyVaultConfiguration>()
    .Bind(builder.Configuration.GetSection("AzureKeyVault"));
builder.Services.AddScoped<Lib.Azure.KeyVaultManager>();
#endregion

#region  Email
builder.Services.AddOptions<Lib.Email.Configuration>()
    .Bind(builder.Configuration.GetSection("EmailSender"));
builder.Services.AddScoped<EmailService>();
#endregion

#region  Jobs
builder.Services.AddHostedService<DocuSignReader>();
builder.Services.AddHostedService<ContactCreator>();
builder.Services.AddHostedService<ContactCanceller>();
builder.Services.AddHostedService<EmailSender>();
builder.Services.AddHostedService<DocuSignContractUploader>();
builder.Services.AddHostedService<SignRemainder>();
#endregion

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();