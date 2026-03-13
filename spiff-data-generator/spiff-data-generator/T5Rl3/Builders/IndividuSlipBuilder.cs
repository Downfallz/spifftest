using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.RandomGen;
using spiff_data_generator.T5Rl3.Config;
using spiff_data_generator.T5Rl3.Models;

namespace spiff_data_generator.T5Rl3.Builders;

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
                ["codFormulaireReleve"] = context.IsQc ? "T5RL3" : "T5",
                ["codLangue"] = context.Langue,
                ["codDevise"] = context.Devise,
                ["typImpression"] = context.TypImpression,
                ["holdMail"] = context.HoldMail,
                ["numIdentificationEmetteur"] = context.NumTransit,
                ["parties"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["idCodSousTypePartie"] = 1,
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
                        ["adresseFiscale"] = AdresseBuilder.Build(_random, context),
                        ["indAdFiscalePostaleIdentique"] = true,
                    }
                },
                ["documents"] = BuildDocuments(context)
            },
            ["contenu"] = new Dictionary<string, object>
            {
                ["cases"] = CaseBuilder.Build(context)
            }
        };
    }

    private List<object> BuildDocuments(SlipContext context)
    {
        return
        [
            new Dictionary<string, object>
            {
                ["metaDonneesDocument"] = new List<object>
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
                        ["valMetadonneeDocument"] = context.NumCompte.PadLeft(7, '0')
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
