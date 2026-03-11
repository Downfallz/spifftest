using spiff_data_generator.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SPIFF Data Generator API",
        Version = "v1",
        Description = "POC - Génération de fichiers T5RL3/T5 via API REST"
    });
});

builder.Services.AddSingleton<IFtpService, LocalDiskFtpService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
