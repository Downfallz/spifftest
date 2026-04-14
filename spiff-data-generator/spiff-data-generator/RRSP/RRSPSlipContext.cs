using spiff_data_generator.Common.Models;

namespace spiff_data_generator.RRSP;

public sealed record RRSPSlipContext(
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
