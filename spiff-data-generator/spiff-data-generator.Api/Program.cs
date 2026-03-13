using System.Reflection;
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
        Description = "POC - Génération de fichiers T5RL3/T5 via API REST. "
            + "POST /api/generate avec un body vide {} pour générer 20 feuillets avec les paramètres par défaut."
    });
    // Include XML comments from API project
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    // Include XML comments from core project (AnomalyConfig, etc.)
    var coreXml = Path.Combine(AppContext.BaseDirectory, "spiff-data-generator.xml");
    if (File.Exists(coreXml))
        c.IncludeXmlComments(coreXml);
});

builder.Services.AddSingleton<IFtpService, LocalDiskFtpService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
