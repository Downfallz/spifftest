using Bogus;
using Microsoft.Extensions.DependencyInjection;
using spiff_data_generator.Common.Config;
using spiff_data_generator.Common.Export;
using spiff_data_generator.Common.Logging;
using System.Diagnostics;

namespace spiff_data_generator;

public static class GeneratorRunner
{
    public static (string zipPath, string outputDir) Run(
        string typeFeuillet,
        GeneratorConfig config)
    {
        try
        {
            // Reset seed to respect inline overrides
            Randomizer.Seed = new Random(config.Seed);

            var prefix = config.GetNextFilePrefix(DateTime.Now.ToString("yyyyMMdd"));

            using var logger = new FileGenerationLogger(config.OutputDir, prefix);
            using var provider = ServiceProviderFactory.Build(typeFeuillet, config, logger);

            var exporter = provider.GetRequiredService<IZipExporter>();
            var sw = Stopwatch.StartNew();

            ConsoleUi.RunProgress(exporter, config.NombreLignes, typeFeuillet);

            sw.Stop();

            return ConsoleUi.ShowSummary(exporter, config, prefix, sw);
        }
        catch (Exception ex)
        {
            ConsoleUi.DisplayError(ex);
            return (string.Empty, config.OutputDir);
        }
    }
}
