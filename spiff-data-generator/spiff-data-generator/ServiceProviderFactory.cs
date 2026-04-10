using Microsoft.Extensions.DependencyInjection;
using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.Common.Config;
using spiff_data_generator.Common.Export;
using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.Logging;
using spiff_data_generator.Common.RandomGen;
using spiff_data_generator.T5Rl3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spiff_data_generator;
public static class ServiceProviderFactory
{
    public static ServiceProvider Build(string typeFeuillet,
        GeneratorConfig config,
        IGenerationLogger logger)
    {
        var services = new ServiceCollection();
        // ── Common services ────────────────────────────────────────
        services.AddSingleton(config);
        services.AddSingleton(logger);
        services.AddSingleton<IRandomService, RandomService>();
        services.AddSingleton<IZipExporter, ZipExporter>();
        services.AddSingleton<IAnomalyService, AnomalyService>();

        // ── Type-specific services ─────────────────────────────────
        switch (typeFeuillet)
        {
            case "T5RL3":
                services.AddSingleton<ISlipBuilder<T5RL3SlipContext>, T5RL3IndividuSlipBuilder>();
                services.AddSingleton<ISlipBuilder<T5RL3SlipContext>, T5RL3OrganisationSlipBuilder>();
                services.AddSingleton<ISlipGenerator, T5RL3SlipGenerator>();
                break;
            default:
                throw new ArgumentException($"Unknown typeFeuillet: {typeFeuillet}");
        }
        return services.BuildServiceProvider();
    }
}
