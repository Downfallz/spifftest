using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.RandomGen;
using spiff_data_generator.T5Rl3.Config;
using spiff_data_generator.T5Rl3.Models;

namespace spiff_data_generator.T5Rl3.Builders;

public sealed class OrganisationSlipBuilder : ISlipBuilder
{
    private readonly IRandomService _random;

    public OrganisationSlipBuilder(IRandomService random)
    {
        _random = random;
    }

    public bool CanBuild(SlipContext context) => !context.IsIndividu;

    public Dictionary<string, object> Build(SlipContext context)
    {
        var genre = _random.RandomChoice<OrganisationType>(Constants.TypesOrganisation);
        string ne = _random.FixedDigits(9);
        string neq = _random.GenerateNEQ(genre);
        string fid = "T" + _random.FixedDigits(8);
        string ni = _random.GenerateNI();
        string nom1 = _random.CompanyName();
        string nom2 = _random.CompanySuffix();

        var identification = BuildIdentification(context, genre, ne, neq, fid, ni);
        var documents = BuildDocuments(context, genre);

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
                        ["idCodSousTypePartie"] = 2,
                        ["idCodRoleRelevePartie"] = 1,
                        ["idCodTypeRoleRelevePartie"] = (int)genre,
                        ["identificationPartie"] = identification,
                        ["nomOrganisationLign1"] = nom1,
                        ["nomOrganisationLign2"] = nom2,
                        ["adresseFiscale"] = AdresseHelper.BuildAdresse(_random, context),
                        ["indAdFiscalePostaleIdentique"] = true,
                    }
                },
                ["documents"] = documents
            },
            ["contenu"] = new Dictionary<string, object>
            {
                ["cases"] = CaseBuilder.Build(context)
            }
        };
    }

    private List<object> BuildIdentification(
        SlipContext context, OrganisationType genre, string ne, string neq, string fid, string ni)
    {
        var identification = new List<object>
        {
            new Dictionary<string, object>
            {
                ["idCodTypeIdentificationPartie"] = 4,
                ["numIdentificationPartie"] = context.NumTransit + context.NumCompte
            }
        };

        if (genre is OrganisationType.Societe or OrganisationType.Association)
        {
            identification.Add(new Dictionary<string, object>
            {
                ["idCodTypeIdentificationPartie"] = 2, // NE
                ["numIdentificationPartie"] = ne
            });
        }

        if (genre == OrganisationType.Fiducie)
        {
            if (context.IsQc)
            {
                // Fiducie QC: NEQ + NI
                identification.Add(new Dictionary<string, object>
                {
                    ["idCodTypeIdentificationPartie"] = 6, // NEQ
                    ["numIdentificationPartie"] = neq
                });
                identification.Add(new Dictionary<string, object>
                {
                    ["idCodTypeIdentificationPartie"] = 7, // NI
                    ["numIdentificationPartie"] = ni
                });
            }
            else
            {
                // Fiducie hors QC: FID
                identification.Add(new Dictionary<string, object>
                {
                    ["idCodTypeIdentificationPartie"] = 8, // FID
                    ["numIdentificationPartie"] = fid
                });
            }
        }
        else if (context.IsQc)
        {
            // Non-Fiducie QC: NEQ seulement
            identification.Add(new Dictionary<string, object>
            {
                ["idCodTypeIdentificationPartie"] = 6, // NEQ
                ["numIdentificationPartie"] = neq
            });
        }

        return identification;
    }

    private List<object> BuildDocuments(SlipContext context, OrganisationType genre)
    {
        var metadonnees = new List<object>
        {
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
            },
        };

        if (genre == OrganisationType.Fiducie)
        {
            metadonnees.Add(new Dictionary<string, object>
            {
                ["codTypeMetadonneeDocument"] = "numIdentifiantPDO",
                ["valMetadonneeDocument"] = _random.FixedDigits(11)
            });
        }

        return
        [
            new Dictionary<string, object>
            {
                ["metadonneesDocument"] = metadonnees
            }
        ];
    }

}
