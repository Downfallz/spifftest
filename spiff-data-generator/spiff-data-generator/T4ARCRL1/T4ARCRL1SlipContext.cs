using spiff_data_generator.Common.Models;

namespace spiff_data_generator.T4ARCRL1;

public sealed record T4ARCRL1SlipContext(
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
