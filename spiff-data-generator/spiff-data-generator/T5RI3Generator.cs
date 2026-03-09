using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Bogus;
using spiff_data_generator;

public class T5RI3Generator
{
    private readonly T5RI3Config _cfg;
    private readonly Random _rng;
    private readonly Faker _faker;

    public T5RI3Generator(T5RI3Config cfg)
    {
        _cfg = cfg;
        _rng = new Random(cfg.Seed);
        _faker = new Faker("fr");
    }

    public void GenerateToFile()
    {
        Directory.CreateDirectory(_cfg.OutputDir);
        var prefix = _cfg.GetOutputPrefix();
        var zipPath = Path.Combine(_cfg.OutputDir, $"{prefix}.zip");

        using var fs = File.Create(zipPath);
        using var archive = new ZipArchive(fs, ZipArchiveMode.Create, leaveOpen: false);

        int fileIndex = 1;
        int linesWritten = 0;
        int batchCount = 0;

        var individuIds = GenerateIndividuIds(_cfg.NombreIndividus);

        while (linesWritten < _cfg.NombreLignes)
        {
            int batchSize = Math.Min(_cfg.BatchSize, _cfg.NombreLignes - linesWritten);
            var entry = archive.CreateEntry($"{prefix}_{fileIndex:D4}.json", CompressionLevel.Optimal);

            using (var entryStream = entry.Open())
            using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
            {
                var records = new List<object>(batchSize);
                for (int i = 0; i < batchSize; i++)
                {
                    records.Add(GenerateRecord(individuIds, linesWritten + i + 1));
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = _cfg.PrettyPrint,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                var json = JsonSerializer.Serialize(records, options);
                writer.Write(json);
            }

            linesWritten += batchSize;
            batchCount++;
            fileIndex++;
            Console.WriteLine($"  Batch {batchCount}: {linesWritten}/{_cfg.NombreLignes} lignes");
        }

        Console.WriteLine($"Fichier généré: {zipPath}");
    }

    private List<string> GenerateIndividuIds(int count)
    {
        var ids = new HashSet<string>();
        while (ids.Count < count)
        {
            ids.Add(RandomUtils.FixedDigits(_rng, 9));
        }
        return ids.ToList();
    }

    private object GenerateRecord(List<string> individuIds, int lineNumber)
    {
        var individu = RandomUtils.RandomChoice(_rng, individuIds);
        var typeOrg = RandomUtils.RandomChoice(_rng, Constants.TYPES_ORGANISATION);

        var transitNumbers = _cfg.IndicateurOntario
            ? Constants.NUMEROS_INSTITUTION_TRANSIT_ONTARIO
            : Constants.NUMEROS_INSTITUTION_TRANSIT;
        var transit = RandomUtils.RandomChoice(_rng, transitNumbers);

        var courrierRetenu = RandomUtils.WeightedChoice(
            _rng,
            new[] { "O", "N" },
            _cfg.WeightsCourrierRetenu);

        var impression = RandomUtils.WeightedChoice(
            _rng,
            new[] { "O", "N" },
            _cfg.WeightsImpression);

        var codeProvince = RandomUtils.WeightedChoice(
            _rng,
            new[] { "QC", "ON" },
            _cfg.WeightsCodeProvince);

        var devise = RandomUtils.RandomChoice(_rng, _cfg.Devises);
        var montant = Math.Round(_rng.NextDouble() * 50000, 2);
        var noFeuillet = _rng.Next(1, _cfg.NombreFeuilletParCaisse + 1);

        return new
        {
            NumeroLigne = lineNumber,
            Plateforme = _cfg.Plateforme,
            CodeSysteme = _cfg.CodeSysteme,
            TypeDeclaration = _cfg.TypeDeclaration,
            CycleProduction = _cfg.CycleProduction,
            AnneeProduction = _cfg.AnneeProduction,
            IdentifiantIndividu = individu,
            TypeOrganisation = typeOrg,
            NumeroInstitutionTransit = transit,
            CourrierRetenu = courrierRetenu,
            Impression = impression,
            CodeProvince = codeProvince,
            Devise = devise,
            Montant = montant,
            NumeroFeuillet = noFeuillet
        };
    }
}
