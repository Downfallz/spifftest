using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.Common.Export;
using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.Logging;
using spiff_data_generator.Common.RandomGen;
using spiff_data_generator.T5Rl3.Builders;
using spiff_data_generator.T5Rl3.Config;
using spiff_data_generator.T5Rl3.Generation;
using Xunit;

namespace spiff_data_generator.Tests.Integration;

public class SlipJsonSchemaTests
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

    private static T5Rl3Config SmallConfig() => new()
    {
        Seed = 42,
        NombreIndividus = 3,
        NombreLignes = 6,
        BatchSize = 10,
        WeightsCourrierRetenu = [50, 50],
        WeightsImpression = [50, 50],
        WeightsCodeProvince = [50, 50],
    };

    [Fact]
    public void ZipContainsValidJsonArrays()
    {
        var config = SmallConfig();
        using var sp = BuildServices(config);
        var exporter = sp.GetRequiredService<IZipExporter>();

        using var ms = new MemoryStream();
        exporter.ExportToStream(ms, "20260101");
        ms.Position = 0;

        using var zip = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Read);
        zip.Entries.Should().NotBeEmpty();

        foreach (var entry in zip.Entries)
        {
            entry.Name.Should().EndWith(".json");
            using var reader = new StreamReader(entry.Open());
            var json = reader.ReadToEnd();
            var array = JArray.Parse(json);
            array.Should().NotBeEmpty();
        }
    }

    [Fact]
    public void IndividuSlip_HasExpectedStructure()
    {
        using var sp = BuildServices(SmallConfig());
        var generator = sp.GetRequiredService<ISlipGenerator>();

        var slip = generator.Generate(1); // individu
        var json = JObject.FromObject(slip);

        // Root keys
        json.Should().ContainKey("information");
        json.Should().ContainKey("contenu");

        // Information block
        var info = json["information"]!;
        info["codFormulaireReleve"]!.Type.Should().Be(JTokenType.String);
        info["codLangue"]!.Type.Should().Be(JTokenType.String);
        info["codDevise"]!.Type.Should().Be(JTokenType.String);
        info["typImpression"]!.Type.Should().Be(JTokenType.String);
        info["holdMail"]!.Type.Should().Be(JTokenType.Boolean);
        info["numIdentificationEmetteur"]!.Type.Should().Be(JTokenType.String);

        // Parties
        var parties = (JArray)info["parties"]!;
        parties.Should().HaveCount(1);
        var party = (JObject)parties[0];
        party.Should().ContainKey("idCodSousTypePartie");
        party.Should().ContainKey("idCodRoleRelevePartie");
        party.Should().ContainKey("idCodTypeRoleRelevePartie");
        party.Should().ContainKey("identificationPartie");
        party.Should().ContainKey("prn");
        party.Should().ContainKey("nomFamille");
        party.Should().ContainKey("nomInitiale");
        party.Should().ContainKey("adresseFiscale");
        party.Should().ContainKey("indAdFiscalePostaleIdentique");

        // Address
        var adresse = (JObject)party["adresseFiscale"]!;
        adresse.Should().ContainKey("numCivique");
        adresse.Should().ContainKey("nomRue");
        adresse.Should().ContainKey("nomMunicipalite");
        adresse.Should().ContainKey("numUnite");
        adresse.Should().ContainKey("codProvince");
        adresse.Should().ContainKey("codPaysIso");
        adresse.Should().ContainKey("numCodPostal");

        // Identification — at least SIN (type 1) and transit+compte (type 4)
        var idents = (JArray)party["identificationPartie"]!;
        idents.Count.Should().BeGreaterOrEqualTo(2);
        foreach (var ident in idents)
        {
            ((JObject)ident).Should().ContainKey("idCodTypeIdentificationPartie");
            ((JObject)ident).Should().ContainKey("numIdentificationPartie");
        }

        // Documents — metadonneesDocument (consistent key)
        var documents = (JArray)info["documents"]!;
        documents.Should().HaveCount(1);
        var doc = (JObject)documents[0];
        doc.Should().ContainKey("metadonneesDocument");

        var metadonnees = (JArray)doc["metadonneesDocument"]!;
        metadonnees.Count.Should().BeGreaterOrEqualTo(1);
        foreach (var meta in metadonnees)
        {
            ((JObject)meta).Should().ContainKey("codTypeMetadonneeDocument");
            ((JObject)meta).Should().ContainKey("valMetadonneeDocument");
        }

        // Contenu / cases
        var contenu = json["contenu"]!;
        contenu.Should().ContainKey("cases");
        var cases = (JArray)contenu["cases"]!;
        cases.Should().NotBeEmpty();
    }

    [Fact]
    public void OrganisationSlip_HasExpectedStructure()
    {
        var config = SmallConfig();
        using var sp = BuildServices(config);
        var generator = sp.GetRequiredService<ISlipGenerator>();

        var slip = generator.Generate(config.NombreIndividus + 1); // organisation
        var json = JObject.FromObject(slip);

        // Root keys
        json.Should().ContainKey("information");
        json.Should().ContainKey("contenu");

        var info = json["information"]!;
        var parties = (JArray)info["parties"]!;
        parties.Should().HaveCount(1);
        var party = (JObject)parties[0];

        // Organisation-specific fields
        party.Should().ContainKey("nomOrganisationLign1");
        party.Should().ContainKey("nomOrganisationLign2");
        party.Should().ContainKey("adresseFiscale");

        // Address uses the consistent key
        var adresse = (JObject)party["adresseFiscale"]!;
        adresse.Should().ContainKey("numCodPostal");

        // Documents use the consistent key
        var documents = (JArray)info["documents"]!;
        documents.Should().HaveCount(1);
        var doc = (JObject)documents[0];
        doc.Should().ContainKey("metadonneesDocument");
    }

    [Fact]
    public void AllSlips_ProduceValidSerializableJson()
    {
        var config = SmallConfig();
        using var sp = BuildServices(config);
        var generator = sp.GetRequiredService<ISlipGenerator>();

        for (int seq = 1; seq <= config.NombreLignes; seq++)
        {
            var slip = generator.Generate(seq);
            var json = JsonConvert.SerializeObject(slip);

            // Must be parseable as valid JSON
            var parsed = JObject.Parse(json);
            parsed.Should().NotBeNull();

            // Must have both root keys
            parsed.Should().ContainKey("information");
            parsed.Should().ContainKey("contenu");
        }
    }
}
