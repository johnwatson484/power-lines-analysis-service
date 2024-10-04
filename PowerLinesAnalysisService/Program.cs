using Microsoft.EntityFrameworkCore;
using PowerLinesAnalysisService.Analysis;
using PowerLinesAnalysisService.Data;
using PowerLinesAnalysisService.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MessageOptions>(builder.Configuration.GetSection(key: "Message"));
builder.Services.Configure<ThresholdOptions>(builder.Configuration.GetSection(key: "Threshold"));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PowerLinesAnalysisService"), options =>
        options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null))
    );

builder.Services.AddScoped<IAnalysisService, AnalysisService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<MessageService>();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

ApplyMigrations(app.Services);

await app.RunAsync();

void ApplyMigrations(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}


