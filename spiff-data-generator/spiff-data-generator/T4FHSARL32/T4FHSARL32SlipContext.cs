using spiff_data_generator.T5Rl3.Models;

namespace spiff_data_generator.T4FHSARL32;

public sealed record T4FHSARL32SlipContext(
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
