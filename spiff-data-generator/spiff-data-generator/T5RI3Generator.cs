using System.Globalization;
using System.IO.Compression;
using Bogus;
using Newtonsoft.Json;

namespace spiff_data_generator;

public class T5Rl3Generator
{
    private readonly T5Rl3Config cfg;
    private readonly Random rng;
    private readonly Faker faker;

    public T5Rl3Generator(T5Rl3Config config)
    {
        cfg = config;
        rng = new Random(cfg.Seed);
        faker = new Faker("en_CA");
    }

    /// <summary>
    /// Génère le ZIP à l'emplacement calculé à partir de la config.
    /// </summary>
    public void GenerateToFile()
    {
        var currentDate = DateTime.Today.ToString(format: "yyyyMMdd");
        var filePrefix = $"{cfg.GetOutputPrefix()}_{currentDate}01";
        var zipPath = Path.Combine(cfg.OutputDir, $"{filePrefix}.zip");
        Directory.CreateDirectory(cfg.OutputDir);

        using var fs = new FileStream(zipPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        GenerateToStream(fs, currentDate);
        Console.WriteLine($"ZIP généré: {zipPath}");
    }

    /// <summary>
    /// Génère le ZIP vers un stream fourni (utile pour un API).
    /// </summary>
    public void GenerateToStream(Stream output, string? yyyymmdd = null)
    {
        var dateString = yyyymmdd ?? DateTime.Today.ToString(format: "yyyyMMdd");
        int jsonFileIndex = 1;

        using var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true);

        var startTime = DateTime.Now;

        for (int i = 0; i < cfg.NombreLignes; i += cfg.BatchSize)
        {
            int startSeq = i + 1;
            int endSeq = Math.Min(startSeq + cfg.BatchSize, 1 + cfg.NombreLignes);

            var entry = zip.CreateEntry($"{cfg.GetOutputPrefix()}_{jsonFileIndex}.json", CompressionLevel.Optimal);
            using var entryStream = entry.Open();
            using var sw = new StreamWriter(entryStream);
            using var jw = new JsonTextWriter(sw) { Formatting = cfg.PrettyPrint ? Formatting.Indented : Formatting.None };

            jw.WriteStartArray();

            for (int seq = startSeq; seq < endSeq; seq++)
            {
                var obj = GenerateObject(seq);
                JsonSerializer.CreateDefault(new JsonSerializerSettings
                {
                    Culture = CultureInfo.InvariantCulture,
                    StringEscapeHandling = StringEscapeHandling.Default
                }).Serialize(jw, obj);
            }

            jw.WriteEndArray();

            jsonFileIndex++;
            PrintProgress(endSeq - 1, cfg.NombreLignes);
        }

        var endTime = DateTime.Now;
        Console.WriteLine($"Génération terminée à: {endTime:HH:mm:ss}");
        Console.WriteLine($"Durée totale: {endTime - startTime}");
    }

    private object GenerateObject(int seq)
    {
        bool isIndividu = seq <= cfg.NombreIndividus;

        // Province (QC vs Autre)
        string province = RandomUtils.WeightedChoice(rng,
            vals: new[] { "QC", "Autre" }, cfg.WeightsCodeProvince);
        if (province == "Autre")
            province = cfg.IndicateurOntario ? "ON" : RandomUtils.RandomChoice(rng,
                new[] { "AB", "BC", "MB", "NB", "NS", "NL", "PE", "SK", "NT", "NU", "YT", "ON" });

        bool isQc = province == "QC";

        // Impression & hold mail
        string typImpression = RandomUtils.WeightedChoice(rng, vals: new[] { "PN", "N" }, cfg.WeightsImpression);
        bool holdMail = RandomUtils.WeightedChoice(rng, vals: new[] { true, false }, cfg.WeightsCourrierRetenu);

        string pays = "CAN";
        string langue = isQc ? "F" : "A";
        string devise = RandomUtils.RandomChoice(rng, cfg.Devises);

        // Transit
        int transitIndex = (seq - 1) / cfg.NombreFeuilletParCaisse;
        string numTransit = cfg.IndicateurOntario
                ? Constants.NUMEROS_INSTITUTION_TRANSIT_ONTARIO[transitIndex % Constants.NUMEROS_INSTITUTION_TRANSIT_ONTARIO.Length]
                : Constants.NUMEROS_INSTITUTION_TRANSIT[transitIndex % Constants.NUMEROS_INSTITUTION_TRANSIT.Length];

        // Compte
        string numCompte = (seq % 999_999 == 0) ? "999999" : (seq % 999_999).ToString("D6", CultureInfo.InvariantCulture);

        // Montants (max 8 entiers + 2 décimales)
        string case13 = RandomUtils.RandomDecimal(rng, minLeft: 1, maxLeft: 8, decimals: 2);
        string caseD = RandomUtils.RandomDecimal(rng, minLeft: 1, maxLeft: 8, decimals: 2);

        Dictionary<string, object> root;
        if (isIndividu)
            root = BuildIndividu(numTransit, numCompte, province, isQc, langue, pays, typImpression, holdMail, devise,
                case13, caseD);
        else
            root = BuildOrganisation(numTransit, numCompte, province, isQc, langue, pays, typImpression, holdMail, devise,
                case13, caseD);

        // Appliquer anomalie si configurée
        var anomaly = GetAnomalyForSeq(seq);
        if (anomaly != null)
            AnomalyApplicator.Apply(root, anomaly, isIndividu);

        return root;
    }

    private string? GetAnomalyForSeq(int seq)
    {
        if (!cfg.Anomalies.Enabled) return null;

        int anomalyStart = cfg.NombreLignes - TotalAnomalyCount + 1;
        if (seq < anomalyStart) return null;

        int offset = seq - anomalyStart;
        foreach (var level in new[] { cfg.Anomalies.Bloquant, cfg.Anomalies.Importante, cfg.Anomalies.SevereImpression, cfg.Anomalies.Avertissement })
        {
            if (level.Nombre <= 0 || level.Types.Length == 0) continue;

            if (offset < level.Nombre)
                return level.Types[offset % level.Types.Length];

            offset -= level.Nombre;
        }

        return null;
    }

    private int TotalAnomalyCount =>
        cfg.Anomalies.Bloquant.Nombre
        + cfg.Anomalies.Importante.Nombre
        + cfg.Anomalies.SevereImpression.Nombre
        + cfg.Anomalies.Avertissement.Nombre;

    private Dictionary<string, object> BuildIndividu(
        string numTransit, string numCompte, string province, bool isQc, string langue, string pays, string typImpression, bool holdMail, string devise,
        string case13, string caseD)
    {
        string prenom = faker.Name.FirstName().ToUpperInvariant();
        string nom = faker.Name.LastName().ToUpperInvariant();

        var root = new Dictionary<string, object>
        {
            ["information"] = new Dictionary<string, object>
            {
                ["codFormulaire"] = isQc ? "T5RL3" : "T5",
                ["codLangue"] = langue,
                ["codDevise"] = devise,
                ["typImpression"] = typImpression,
                ["holdMailIndicateur"] = holdMail,
                ["numIdentification"] = numTransit,
                ["parties"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["isCodSousTypePartie"] = 1,
                        ["idCodRoleRelevePartie"] = 1,
                        ["idCodTypeRoleRelevePartie"] = 1,
                        ["identificationPartie"] = new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                ["idCodTypeIdentificationPartie"] = 1,
                                ["numIdentificationPartie"] = RandomUtils.GenerateSIN(rng)
                            },
                            new Dictionary<string, object>
                            {
                                ["idCodTypeIdentificationPartie"] = 4,
                                ["numIdentificationPartie"] = numTransit + numCompte
                            }
                        },

                        ["prn"] = prenom,
                        ["nomFamille"] = nom,
                        ["nomInitiale"] = prenom.Length > 0 ? prenom.Substring(0,1) : "",
                        ["adresseFiscale"] = new Dictionary<string, object>
                        {
                            ["numCivique"] = faker.Address.BuildingNumber(),
                            ["nomRue"] = faker.Address.StreetName().ToUpperInvariant(),
                            ["nomMunicipalite"] = faker.Address.City().ToUpperInvariant(),
                            ["numUnite"] = faker.Address.SecondaryAddress().ToUpperInvariant(),
                            ["codProvince"] = province,
                            ["numCodePostal"] = RandomUtils.GenerateCanadianPostalCode(rng, province).Replace(" ", ""),
                            ["codePaysIso"] = pays,
                        },

                        ["indAdrFiscalePostaleIdentique"] = true,

                    }
                }
            },
            ["documents"] = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["metadonneesDocument"] = new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            ["codTypeMetadonneeDocument"] = "numIdentifiantPDO",
                            ["valMetadonneeDocument"] = RandomUtils.FixedDigits(rng, length: 11)
                        },
                        new Dictionary<string, object>
                        {
                            ["codTypeMetadonneeDocument"] = "numIdentifiantInstitution",
                            ["valMetadonneeDocument"] = numTransit.Substring(0,3)
                        },
                        new Dictionary<string, object>
                        {
                            ["codTypeMetadonneeDocument"] = "numIdentifiantTransit",
                            ["valMetadonneeDocument"] = numTransit.Substring(3)
                        },
                        new Dictionary<string, object>
                        {
                            ["codTypeMetadonneeDocument"] = "numIdentifiantFolio",
                            ["valMetadonneeDocument"] = numTransit.PadLeft(7,'0')
                        },
                        new Dictionary<string, object>
                        {
                            ["codTypeMetadonneeDocument"] = "codSousTypeDocument",
                            ["valMetadonneeDocument"] = isQc ? "T5R3" : "T5"
                        }
                    }
                }
            },
            ["contenu"] = new Dictionary<string, object>
            {
                ["cases"] = BuildCases(numTransit, numCompte, case13, caseD, isQc) 
            }
        };

        return root;
    }

    private Dictionary<string, object> BuildOrganisation(
        string numTransit, string numCompte, string province, bool isQc, string langue, string pays, string typImpression, bool holdMail, string devise,
        string case13, string caseD)
    {
        int genre = RandomUtils.RandomChoice(rng, vals: Constants.TYPES_ORGANISATION);
        string ne = RandomUtils.FixedDigits(rng, 9);
        string neq = RandomUtils.GenerateNEQ(rng, genre);
        string fid = "T" + RandomUtils.FixedDigits(rng, length: 8);
        string ni = RandomUtils.GenerateNI(rng);

        string nom1 = faker.Company.CompanyName().ToUpperInvariant().Replace(",", "");
        string nom2 = faker.Company.CompanySuffix().ToUpperInvariant().Replace(",", "");

        var identification = new List<object>
        {
            new Dictionary<string, object>
            {
                ["idCodTypeIdentificationPartie"] = 4,
                ["numIdentificationPartie"] = numTransit + numCompte
            }
        };

        var documents = new List<object>
        {
            new Dictionary<string, object>
            {
                ["metaDonneesDocument"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["codTypeMetadonneeDocument"] = "numIdentifiantInstitution",
                        ["valMetadonneeDocument"] = numTransit.Substring(startIndex: 0, length: 3)
                    },
                    new Dictionary<string, object>
                    {
                        ["codTypeMetadonneeDocument"] = "numIdentifiantTransit",
                        ["valMetadonneeDocument"] = numTransit.Substring(3)
                    },
                    new Dictionary<string, object>
                    {
                        ["codTypeMetadonneeDocument"] = "numIdentifiantFolio",
                        ["valMetadonneeDocument"] = numCompte.PadLeft(totalWidth: 7, paddingChar: '0')
                    },
                    new Dictionary<string, object>
                    {
                        ["codTypeMetadonneeDocument"] = "codSousTypeDocument",
                        ["valMetadonneeDocument"] = isQc ? "T5R3" : "T5"
                    },
                }
            }
        };

        if (genre == 3 || genre == 5)
        {
            identification.Add(new Dictionary<string, object>
            {
                ["idCodTypeIdentificationPartie"] = 2, // NE
                ["numIdentificationPartie"] = ne
            });
        }

        if (isQc)
        {
            identification.Add(new Dictionary<string, object>
            {
                ["idCodTypeIdentificationPartie"] = 6, // NEQ
                ["numIdentificationPartie"] = neq
            });
        }
        else if (genre == 4)
        {
            // Ajoute PDO pour fiducie
            ((List<object>)((Dictionary<string, object>)documents[0])["metaDonneesDocument"])
                .Add(new Dictionary<string, object>
                {
                    ["codTypeMetadonneeDocument"] = "numIdentifiantPDO",
                    ["valMetadonneeDocument"] = RandomUtils.FixedDigits(rng, length: 11)
                });

            identification.Add(new Dictionary<string, object>
            {
                ["idCodTypeIdentificationPartie"] = 8, // FID
                ["numIdentificationPartie"] = fid
            });

            if (isQc)
            {
                identification.Add(new Dictionary<string, object>
                {
                    ["idCodTypeIdentificationPartie"] = 7, // NI
                    ["numIdentificationPartie"] = ni
                });
            }
        }





        var root = new Dictionary<string, object>
        {
            ["information"] = new Dictionary<string, object>
            {
                ["codFormulaireReleve"] = isQc ? "T5RL3" : "T5",
                ["codLangue"] = langue,
                ["codDevise"] = devise,
                ["typImpression"] = typImpression,
                ["holdMail"] = holdMail,
                ["numIdentificationEmetteur"] = numTransit,
                ["parties"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["idCodSousTypePartie"] = 2,
                        ["idCodRoleRelevePartie"] = 1,
                        ["idCodTypeRoleRelevePartie"] = genre,
                        ["identificationPartie"] = identification,
                        ["nomOrganisationLign1"] = nom1,
                        ["nomOrganisationLign2"] = nom2,
                        ["adresseFiscale"] = new Dictionary<string, object>
                        {
                            ["numCivique"] = faker.Address.BuildingNumber(),
                            ["nomRue"] = faker.Address.StreetName().ToUpperInvariant(),
                            ["nomMunicipalite"] = faker.Address.City().ToUpperInvariant(),
                            ["numUnite"] = faker.Address.SecondaryAddress().ToUpperInvariant(),
                            ["codProvince"] = province,
                            ["codPaysIso"] = pays,
                            ["numCodPostal"] = RandomUtils.GenerateCanadianPostalCode(rng, province).Replace(" ", ""),
                        },
                        ["indicateurAdresseFiscalePostalIdentique"] = true,
                    }
                },

                ["documents"] = documents
            },


            ["contenu"] = new Dictionary<string, object>
            {
                ["cases"] = BuildCases(numTransit, numCompte, case13, caseD, isQc)
            }
        };

        return root;
    }

    private List<object> BuildCases(string transit, string compte, string case13, string caseD, bool isQc)
    {
        var cases = new List<object>
        {
            new Dictionary<string, object> { ["case"] = "13", ["valeur"] = case13 },
            new Dictionary<string, object> { ["case"] = "2B", ["valeur"] = transit },
            new Dictionary<string, object> { ["case"] = "28", ["valeur"] = compte },
        };

        if (isQc)
        {
            cases.Add(new Dictionary<string, object> { ["case"] = "D", ["valeur"] = caseD });
            cases.Add(new Dictionary<string, object> { ["case"] = "Succ", ["valeur"] = transit });
        }

        return cases;
    }

    private void PrintProgress(int current, int total)
    {
        double pct = current / (double)total * 100.0;
        Console.WriteLine($"Progress: {current}/{total} ({pct:F2}%)");
    }
}
