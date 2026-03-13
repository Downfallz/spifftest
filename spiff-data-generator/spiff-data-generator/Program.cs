using Bogus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using spiff_data_generator;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var config = configuration.GetSection("T5Rl3").Get<T5Rl3Config>() ?? new T5Rl3Config();
Randomizer.Seed = new Random(config.Seed);

var services = new ServiceCollection()
    .AddSingleton(config)
    .AddSingleton<IRandomService, RandomService>()
    .AddSingleton<ISlipBuilder, IndividuSlipBuilder>()
    .AddSingleton<ISlipBuilder, OrganisationSlipBuilder>()
    .AddSingleton<IAnomalyService, AnomalyService>()
    .AddSingleton<ISlipGenerator, SlipGenerator>()
    .AddSingleton<IZipExporter, ZipExporter>()
    .BuildServiceProvider();

var exporter = services.GetRequiredService<IZipExporter>();
exporter.ExportToFile();

Console.WriteLine("Terminé.");
