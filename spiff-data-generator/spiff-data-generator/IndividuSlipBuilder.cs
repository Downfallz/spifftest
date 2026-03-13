namespace spiff_data_generator;

public sealed class IndividuSlipBuilder : ISlipBuilder
{
    private readonly IRandomService _random;

    public IndividuSlipBuilder(IRandomService random)
    {
        _random = random;
    }

    public bool CanBuild(SlipContext context) => context.IsIndividu;

    public Dictionary<string, object> Build(SlipContext context)
    {
        string prenom = _random.FirstName();
        string nom = _random.LastName();

        return new Dictionary<string, object>
        {
            ["information"] = new Dictionary<string, object>
            {
                ["codFormulaire"] = context.IsQc ? "T5RL3" : "T5",
                ["codLangue"] = context.Langue,
                ["codDevise"] = context.Devise,
                ["typImpression"] = context.TypImpression,
                ["holdMailIndicateur"] = context.HoldMail,
                ["numIdentification"] = context.NumTransit,
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
                                ["numIdentificationPartie"] = _random.GenerateSIN()
                            },
                            new Dictionary<string, object>
                            {
                                ["idCodTypeIdentificationPartie"] = 4,
                                ["numIdentificationPartie"] = context.NumTransit + context.NumCompte
                            }
                        },
                        ["prn"] = prenom,
                        ["nomFamille"] = nom,
                        ["nomInitiale"] = prenom.Length > 0 ? prenom[..1] : "",
                        ["adresseFiscale"] = BuildAdresse(context),
                        ["indAdrFiscalePostaleIdentique"] = true,
                    }
                }
            },
            ["documents"] = BuildDocuments(context),
            ["contenu"] = new Dictionary<string, object>
            {
                ["cases"] = CaseBuilder.Build(context)
            }
        };
    }

    private Dictionary<string, object> BuildAdresse(SlipContext context)
    {
        return new Dictionary<string, object>
        {
            ["numCivique"] = _random.BuildingNumber(),
            ["nomRue"] = _random.StreetName(),
            ["nomMunicipalite"] = _random.City(),
            ["numUnite"] = _random.SecondaryAddress(),
            ["codProvince"] = context.Province,
            ["numCodePostal"] = _random.GenerateCanadianPostalCode(context.Province).Replace(" ", ""),
            ["codePaysIso"] = context.Pays,
        };
    }

    private List<object> BuildDocuments(SlipContext context)
    {
        return
        [
            new Dictionary<string, object>
            {
                ["metadonneesDocument"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["codTypeMetadonneeDocument"] = "numIdentifiantPDO",
                        ["valMetadonneeDocument"] = _random.FixedDigits(11)
                    },
                    new Dictionary<string, object>
                    {
                        ["codTypeMetadonneeDocument"] = "numIdentifiantInstitution",
                        ["valMetadonneeDocument"] = context.NumTransit[..3]
                    },
                    new Dictionary<string, object>
                    {
                        ["codTypeMetadonneeDocument"] = "numIdentifiantTransit",
                        ["valMetadonneeDocument"] = context.NumTransit[3..]
                    },
                    new Dictionary<string, object>
                    {
                        ["codTypeMetadonneeDocument"] = "numIdentifiantFolio",
                        ["valMetadonneeDocument"] = context.NumTransit.PadLeft(7, '0')
                    },
                    new Dictionary<string, object>
                    {
                        ["codTypeMetadonneeDocument"] = "codSousTypeDocument",
                        ["valMetadonneeDocument"] = context.IsQc ? "T5R3" : "T5"
                    }
                }
            }
        ];
    }
}
