using Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddDbContext<DbContext, Database.ElectronicSignatureContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOptions<Lib.DocuSign.Configuration>()
    .Bind(builder.Configuration.GetSection("DocuSign"));
builder.Services.AddScoped<Lib.DocuSign.ClientManager>();
builder.Services.AddScoped<DocuSignService>();

builder.Services.AddOptions<Lib.BestSign.Configuration>()
    .Bind(builder.Configuration.GetSection("BestSign"));
builder.Services.AddScoped<Lib.BestSign.TokenManager>();
builder.Services.AddScoped<BestSignService>();

builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<TemplateMappingService>();
builder.Services.AddScoped<DocumentService>();

builder.Services.AddOptions<Lib.Email.Configuration>()
    .Bind(builder.Configuration.GetSection("EmailSender"));
builder.Services.AddScoped<EmailService>();

// builder.Services.AddHostedService<DocuSignReader>();
// builder.Services.AddHostedService<ContactCreator>();
// builder.Services.AddHostedService<ContactCanceller>();
builder.Services.AddHostedService<EmailSender>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();