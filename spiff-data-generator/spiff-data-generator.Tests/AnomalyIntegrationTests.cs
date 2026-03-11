using FluentAssertions;
using spiff_data_generator;
using Xunit;

namespace spiff_data_generator.Tests;

public class AnomalyIntegrationTests
{
    private static T5Rl3Config CreateSmallConfig(bool anomaliesEnabled = false, AnomalyConfig? anomalies = null)
    {
        var cfg = new T5Rl3Config
        {
            Seed = 42,
            NombreIndividus = 5,
            NombreLignes = 10,
            BatchSize = 100,
            WeightsCourrierRetenu = [5, 95],
            WeightsImpression = [80, 20],
            WeightsCodeProvince = [70, 30],
            PrettyPrint = false,
            OutputDir = "out/test",
        };

        if (anomalies != null)
        {
            cfg.Anomalies = anomalies;
        }
        else
        {
            cfg.Anomalies = new AnomalyConfig { Enabled = anomaliesEnabled };
        }

        return cfg;
    }

    [Fact]
    public void Generator_WithoutAnomalies_ShouldProduceValidZip()
    {
        var cfg = CreateSmallConfig(anomaliesEnabled: false);
        var gen = new T5Rl3Generator(cfg);

        using var ms = new MemoryStream();
        var act = () => gen.GenerateToStream(ms);

        act.Should().NotThrow();
        ms.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Generator_WithAnomaliesEnabled_ShouldProduceValidZip()
    {
        var anomCfg = new AnomalyConfig
        {
            Enabled = true,
            Bloquant = new AnomalyLevelConfig
            {
                Nombre = 2,
                Types = new[] { "NomBeneficiaireManquant", "CodeDeviseErrone" }
            },
            Importante = new AnomalyLevelConfig
            {
                Nombre = 1,
                Types = new[] { "NASManquant" }
            },
            SevereImpression = new AnomalyLevelConfig
            {
                Nombre = 1,
                Types = new[] { "CodePostalManquant" }
            },
            Avertissement = new AnomalyLevelConfig
            {
                Nombre = 1,
                Types = new[] { "CodeLangueManquant" }
            }
        };

        var cfg = CreateSmallConfig(anomalies: anomCfg);
        var gen = new T5Rl3Generator(cfg);

        using var ms = new MemoryStream();
        var act = () => gen.GenerateToStream(ms);

        act.Should().NotThrow();
        ms.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Generator_AnomalyCountShouldNotExceedTotalLines()
    {
        var anomCfg = new AnomalyConfig
        {
            Enabled = true,
            Bloquant = new AnomalyLevelConfig
            {
                Nombre = 3,
                Types = new[] { "NomBeneficiaireManquant" }
            }
        };

        // 10 lignes, 3 anomalies = 7 normales + 3 avec anomalie
        var cfg = CreateSmallConfig(anomalies: anomCfg);
        var gen = new T5Rl3Generator(cfg);

        using var ms = new MemoryStream();
        var act = () => gen.GenerateToStream(ms);

        act.Should().NotThrow();
    }

    [Fact]
    public void Generator_ZeroNombreAnomalies_ShouldBehaveAsDisabled()
    {
        var anomCfg = new AnomalyConfig
        {
            Enabled = true,
            Bloquant = new AnomalyLevelConfig { Nombre = 0, Types = new[] { "NomBeneficiaireManquant" } },
        };

        var cfg = CreateSmallConfig(anomalies: anomCfg);
        var gen = new T5Rl3Generator(cfg);

        using var ms = new MemoryStream();
        var act = () => gen.GenerateToStream(ms);

        act.Should().NotThrow();
    }

    [Fact]
    public void Generator_EmptyTypes_ShouldNotCrash()
    {
        var anomCfg = new AnomalyConfig
        {
            Enabled = true,
            Bloquant = new AnomalyLevelConfig { Nombre = 5, Types = [] },
        };

        var cfg = CreateSmallConfig(anomalies: anomCfg);
        var gen = new T5Rl3Generator(cfg);

        using var ms = new MemoryStream();
        var act = () => gen.GenerateToStream(ms);

        act.Should().NotThrow();
    }
}
