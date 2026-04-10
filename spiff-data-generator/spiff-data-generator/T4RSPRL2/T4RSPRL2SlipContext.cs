using spiff_data_generator.Common.Models;

namespace spiff_data_generator.T4RSPRL2;

public sealed record T4RSPRL2SlipContext(
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
