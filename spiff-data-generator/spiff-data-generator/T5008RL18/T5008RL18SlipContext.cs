using spiff_data_generator.T5Rl3.Models;

namespace spiff_data_generator.T5008RL18;

public sealed record T5008RL18SlipContext(
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
