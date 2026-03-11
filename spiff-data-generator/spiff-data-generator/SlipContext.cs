namespace spiff_data_generator;

public sealed record SlipContext(
    string NumTransit,
    string NumCompte,
    string Province,
    bool IsQc,
    string Langue,
    string Pays,
    string TypImpression,
    bool HoldMail,
    string Devise,
    string Case13,
    string CaseD,
    bool IsIndividu);
