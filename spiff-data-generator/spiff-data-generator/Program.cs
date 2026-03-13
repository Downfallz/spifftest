using System.Globalization;
using Bogus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.Common.Export;
using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.Logging;
using spiff_data_generator.Common.RandomGen;
using spiff_data_generator.T5Rl3.Builders;
using spiff_data_generator.T5Rl3.Config;
using spiff_data_generator.T5Rl3.Generation;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var config = configuration.GetSection("T5Rl3").Get<T5Rl3Config>() ?? new T5Rl3Config();
Randomizer.Seed = new Random(config.Seed);

var currentDate = DateTime.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
var filePrefix = $"{config.GetOutputPrefix()}_{currentDate}01";
using var logger = new FileGenerationLogger(config.OutputDir, filePrefix);

var services = new ServiceCollection()
    .AddSingleton(config)
    .AddSingleton<IGenerationLogger>(logger)
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
