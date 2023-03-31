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
builder.Services.AddScoped<Lib.DocuSign.DocuSignService>();

builder.Services.AddOptions<Lib.BestSign.Configuration>()
    .Bind(builder.Configuration.GetSection("BestSign"));
builder.Services.AddScoped<Lib.BestSign.TokenManager>();
builder.Services.AddScoped<Lib.BestSign.ApiClient>();

builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<DocumentService>();

// builder.Services.AddHostedService<DocuSignReader>();
builder.Services.AddHostedService<ContactCreator>();
//builder.Services.AddHostedService<ContactCanceller>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();