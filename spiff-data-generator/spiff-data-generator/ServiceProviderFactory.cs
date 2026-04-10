using Microsoft.Extensions.DependencyInjection;
using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.Common.Config;
using spiff_data_generator.Common.Export;
using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.Logging;
using spiff_data_generator.Common.RandomGen;
using spiff_data_generator.NR4;
using spiff_data_generator.RRSP;
using spiff_data_generator.T4ARCRL1;
using spiff_data_generator.T4FHSARL32;
using spiff_data_generator.T4RIFRL2;
using spiff_data_generator.T4RSPRL2;
using spiff_data_generator.T5008RL18;
using spiff_data_generator.T5Rl3;

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
            case "NR4":
                services.AddSingleton<ISlipBuilder<NR4SlipContext>, NR4IndividuSlipBuilder>();
                services.AddSingleton<ISlipBuilder<NR4SlipContext>, NR4OrganisationSlipBuilder>();
                services.AddSingleton<ISlipGenerator, NR4SlipGenerator>();
                break;
            case "RRSP":
                services.AddSingleton<ISlipBuilder<RRSPSlipContext>, RRSPIndividuSlipBuilder>();
                services.AddSingleton<ISlipBuilder<RRSPSlipContext>, RRSPOrganisationSlipBuilder>();
                services.AddSingleton<ISlipGenerator, RRSPSlipGenerator>();
                break;
            case "T4RIFRL2":
                services.AddSingleton<ISlipBuilder<T4RIFRL2SlipContext>, T4RIFRL2IndividuSlipBuilder>();
                services.AddSingleton<ISlipBuilder<T4RIFRL2SlipContext>, T4RIFRL2OrganisationSlipBuilder>();
                services.AddSingleton<ISlipGenerator, T4RIFRL2SlipGenerator>();
                break;
            case "T4RSPRL2":
                services.AddSingleton<ISlipBuilder<T4RSPRL2SlipContext>, T4RSPRL2IndividuSlipBuilder>();
                services.AddSingleton<ISlipBuilder<T4RSPRL2SlipContext>, T4RSPRL2OrganisationSlipBuilder>();
                services.AddSingleton<ISlipGenerator, T4RSPRL2SlipGenerator>();
                break;
            case "T5008RL18":
                services.AddSingleton<ISlipBuilder<T5008RL18SlipContext>, T5008RL18IndividuSlipBuilder>();
                services.AddSingleton<ISlipBuilder<T5008RL18SlipContext>, T5008RL18OrganisationSlipBuilder>();
                services.AddSingleton<ISlipGenerator, T5008RL18SlipGenerator>();
                break;
            case "T4FHSARL32":
                services.AddSingleton<ISlipBuilder<T4FHSARL32SlipContext>, T4FHSARL32IndividuSlipBuilder>();
                services.AddSingleton<ISlipBuilder<T4FHSARL32SlipContext>, T4FHSARL32OrganisationSlipBuilder>();
                services.AddSingleton<ISlipGenerator, T4FHSARL32SlipGenerator>();
                break;
            case "T4ARCRL1":
                services.AddSingleton<ISlipBuilder<T4ARCRL1SlipContext>, T4ARCRL1IndividuSlipBuilder>();
                services.AddSingleton<ISlipBuilder<T4ARCRL1SlipContext>, T4ARCRL1OrganisationSlipBuilder>();
                services.AddSingleton<ISlipGenerator, T4ARCRL1SlipGenerator>();
                break;
            default:
                throw new ArgumentException($"Unknown typeFeuillet: {typeFeuillet}");
        }
        return services.BuildServiceProvider();
    }
}
