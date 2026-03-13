using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.Common.Export;
using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.Logging;
using spiff_data_generator.Common.Random;
using spiff_data_generator.T5Rl3.Builders;
using spiff_data_generator.T5Rl3.Config;
using spiff_data_generator.T5Rl3.Generation;
using Xunit;

namespace spiff_data_generator.Tests;

public class AnomalyIntegrationTests
{
    private static ServiceProvider BuildServices(T5Rl3Config config)
    {
        Randomizer.Seed = new Random(config.Seed);
        return new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<IRandomService, RandomService>()
            .AddSingleton<ISlipBuilder, IndividuSlipBuilder>()
            .AddSingleton<ISlipBuilder, OrganisationSlipBuilder>()
            .AddSingleton<IAnomalyService, AnomalyService>()
            .AddSingleton<IGenerationLogger, NullGenerationLogger>()
            .AddSingleton<ISlipGenerator, SlipGenerator>()
            .AddSingleton<IZipExporter, ZipExporter>()
            .BuildServiceProvider();
    }

    private static T5Rl3Config SmallConfig(AnomalyConfig? anomalies = null) => new()
    {
        Seed = 42,
        NombreIndividus = 5,
        NombreLignes = 10,
        BatchSize = 100,
        PrettyPrint = false,
        OutputDir = "out/test",
        Anomalies = anomalies ?? new AnomalyConfig(),
    };

    [Fact]
    public void ExportToStream_WithoutAnomalies_ProducesValidZip()
    {
        var cfg = SmallConfig();
        using var sp = BuildServices(cfg);
        var exporter = sp.GetRequiredService<IZipExporter>();

        using var ms = new MemoryStream();
        var act = () => exporter.ExportToStream(ms);

        act.Should().NotThrow();
        ms.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ExportToStream_WithAnomalies_ProducesValidZip()
    {
        var cfg = SmallConfig(new AnomalyConfig
        {
            Enabled = true,
            Bloquant = new AnomalyLevelConfig
            {
                Nombre = 2,
                Types = [AnomalyKind.NomBeneficiaireManquant, AnomalyKind.CodeDeviseErrone]
            },
            Importante = new AnomalyLevelConfig
            {
                Nombre = 1,
                Types = [AnomalyKind.NASManquant]
            },
            SevereImpression = new AnomalyLevelConfig
            {
                Nombre = 1,
                Types = [AnomalyKind.CodePostalManquant]
            },
            Avertissement = new AnomalyLevelConfig
            {
                Nombre = 1,
                Types = [AnomalyKind.CodeLangueManquant]
            }
        });
        using var sp = BuildServices(cfg);
        var exporter = sp.GetRequiredService<IZipExporter>();

        using var ms = new MemoryStream();
        var act = () => exporter.ExportToStream(ms);

        act.Should().NotThrow();
        ms.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ExportToStream_ZeroNombre_BehavesAsDisabled()
    {
        var cfg = SmallConfig(new AnomalyConfig
        {
            Enabled = true,
            Bloquant = new AnomalyLevelConfig { Nombre = 0, Types = [AnomalyKind.NomBeneficiaireManquant] },
        });
        using var sp = BuildServices(cfg);
        var exporter = sp.GetRequiredService<IZipExporter>();

        using var ms = new MemoryStream();
        var act = () => exporter.ExportToStream(ms);

        act.Should().NotThrow();
    }

    [Fact]
    public void ExportToStream_EmptyTypes_DoesNotCrash()
    {
        var cfg = SmallConfig(new AnomalyConfig
        {
            Enabled = true,
            Bloquant = new AnomalyLevelConfig { Nombre = 5, Types = [] },
        });
        using var sp = BuildServices(cfg);
        var exporter = sp.GetRequiredService<IZipExporter>();

        using var ms = new MemoryStream();
        var act = () => exporter.ExportToStream(ms);

        act.Should().NotThrow();
    }

    [Fact]
    public void SlipGenerator_ProducesIndividuAndOrganisation()
    {
        var cfg = SmallConfig();
        using var sp = BuildServices(cfg);
        var generator = sp.GetRequiredService<ISlipGenerator>();

        var individu = generator.Generate(1); // seq 1 <= 5 individus
        var organisation = generator.Generate(6); // seq 6 > 5 individus

        individu.Should().ContainKey("information");
        ((Dictionary<string, object>)individu["information"]).Should().ContainKey("codFormulaireReleve");

        organisation.Should().ContainKey("information");
        ((Dictionary<string, object>)organisation["information"]).Should().ContainKey("codFormulaireReleve");
    }
}
