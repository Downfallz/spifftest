using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.Logging;
using spiff_data_generator.Common.Random;
using spiff_data_generator.T5Rl3.Builders;
using spiff_data_generator.T5Rl3.Config;
using spiff_data_generator.T5Rl3.Generation;
using Xunit;

namespace spiff_data_generator.Tests.T5Rl3.Generation;

public class SlipGeneratorTests
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
            .BuildServiceProvider();
    }

    private static T5Rl3Config SmallConfig() => new()
    {
        Seed = 42,
        NombreIndividus = 5,
        NombreLignes = 10,
        BatchSize = 100,
        WeightsCourrierRetenu = [50, 50],
        WeightsImpression = [50, 50],
        WeightsCodeProvince = [50, 50],
    };

    [Fact]
    public void Generate_Individu_HasCodFormulaireReleve()
    {
        using var sp = BuildServices(SmallConfig());
        var generator = sp.GetRequiredService<ISlipGenerator>();

        var result = generator.Generate(1);
        var info = (Dictionary<string, object>)result["information"];

        info.Should().ContainKey("codFormulaireReleve");
    }

    [Fact]
    public void Generate_Organisation_HasCodFormulaireReleve()
    {
        using var sp = BuildServices(SmallConfig());
        var generator = sp.GetRequiredService<ISlipGenerator>();

        var result = generator.Generate(6);
        var info = (Dictionary<string, object>)result["information"];

        info.Should().ContainKey("codFormulaireReleve");
    }

    [Fact]
    public void Generate_WithEmetteurFourni_AddsIdentification()
    {
        var config = SmallConfig();
        config.AjouterEmetteurFourni = true;

        using var sp = BuildServices(config);
        var generator = sp.GetRequiredService<ISlipGenerator>();

        var result = generator.Generate(1);
        var info = (Dictionary<string, object>)result["information"];
        var parties = (List<object>)info["parties"];
        var party = (Dictionary<string, object>)parties[0];
        var idents = (List<object>)party["identificationPartie"];

        // Should have an extra identification with type 5
        idents.Cast<Dictionary<string, object>>()
            .Should().Contain(i => (int)i["idCodTypeIdentificationPartie"] == 5);
    }

    [Fact]
    public void Generate_WithoutEmetteurFourni_NoType5()
    {
        var config = SmallConfig();
        config.AjouterEmetteurFourni = false;

        using var sp = BuildServices(config);
        var generator = sp.GetRequiredService<ISlipGenerator>();

        var result = generator.Generate(1);
        var info = (Dictionary<string, object>)result["information"];
        var parties = (List<object>)info["parties"];
        var party = (Dictionary<string, object>)parties[0];
        var idents = (List<object>)party["identificationPartie"];

        idents.Cast<Dictionary<string, object>>()
            .Should().NotContain(i => (int)i["idCodTypeIdentificationPartie"] == 5);
    }

    [Fact]
    public void Generate_WithIdUnique_AddsNumIdentificationUnique()
    {
        var config = SmallConfig();
        config.AjouterIdUnique = true;
        config.PrefixeIdentificationUnique = "ADO-2026-";

        using var sp = BuildServices(config);
        var generator = sp.GetRequiredService<ISlipGenerator>();

        var result = generator.Generate(1);
        var info = (Dictionary<string, object>)result["information"];

        info.Should().ContainKey("numIdentificationUnique");
        info["numIdentificationUnique"].Should().Be("ADO-2026-1");
    }

    [Fact]
    public void Generate_WithoutIdUnique_NoNumIdentificationUnique()
    {
        var config = SmallConfig();
        config.AjouterIdUnique = false;

        using var sp = BuildServices(config);
        var generator = sp.GetRequiredService<ISlipGenerator>();

        var result = generator.Generate(1);
        var info = (Dictionary<string, object>)result["information"];

        info.Should().NotContainKey("numIdentificationUnique");
    }

    [Fact]
    public void Generate_AllSequences_DoNotThrow()
    {
        using var sp = BuildServices(SmallConfig());
        var generator = sp.GetRequiredService<ISlipGenerator>();

        var act = () =>
        {
            for (int i = 1; i <= 10; i++)
                generator.Generate(i);
        };

        act.Should().NotThrow();
    }
}
