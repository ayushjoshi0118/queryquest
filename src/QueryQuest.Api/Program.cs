using Microsoft.EntityFrameworkCore;
using QueryQuest.Api.Data;
using QueryQuest.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Ensure App_Data directory exists
var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
if (!Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
}

// 1. Configure EF Core AppDbContext for application metadata and progress
var appDbPath = Path.Combine(dataDirectory, "queryquest_metadata.db");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite($"Data Source={appDbPath}");
});

// 2. Register Application Services
builder.Services.AddScoped<ISqlExecutionEngine, SqlExecutionEngine>();
builder.Services.AddScoped<IChallengeValidator, ChallengeValidator>();
builder.Services.AddScoped<IProgressService, ProgressService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS for local development flexibility
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 3. Database Initialization: Ensure metadata db created and seed isolated sample educational database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    var sampleDbPath = Path.Combine(dataDirectory, "sample_educational.db");
    SampleDatabaseSeeder.InitializeDatabase(sampleDbPath);
}

// 4. Configure HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Serve Static Files for Frontend SPA
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

// Fallback to index.html for client-side routing
app.MapFallbackToFile("index.html");

app.Run();

// Make Program accessible for WebApplicationFactory in integration tests
public partial class Program { }
