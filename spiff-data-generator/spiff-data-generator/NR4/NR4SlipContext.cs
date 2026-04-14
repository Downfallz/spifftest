using spiff_data_generator.Common.Models;

namespace spiff_data_generator.NR4;

public sealed record NR4SlipContext(
    string NumTransit,
    string NumCompte,
    string Province,
    bool IsQc,
    string Langue,
    string Pays,
    string TypImpression,
    bool HoldMail,
    string Devise,
    bool IsIndividu) : SlipContextBase(Province, Pays);
