public class T5Rl3Config
{
    public string Plateforme { get; set; } = "SPIFF";
    public string CodeSysteme { get; set; } = "D10815";
    public string TypeDeclaration { get; set; } = "O";
    public string CycleProduction { get; set; } = "A";
    public string AnneeProduction { get; set; } = "2026";

    public int Seed { get; set; } = 0;

    public int NombreIndividus { get; set; } = 2_250_000;
    public int NombreLignes { get; set; } = 2_500_000;
    public int BatchSize { get; set; } = 25_000;

    public int[] WeightsCourrierRetenu { get; set; } = [5, 95];
    public int[] WeightsImpression { get; set; } = [80, 20];
    public int[] WeightsCodeProvince { get; set; } = [70, 30];

    public bool IndicateurOntario { get; set; } = false;
    public int NombreFeuilletParCaisse { get; set; } = 999_999;

    public string[] Devises { get; set; } =
        new[] { "CAD", "USD", "AUD", "DKK", "EUR", "GBP", "HKD", "JPY", "NZD" };

    public string OutputDir { get; set; } = "out/T5RL3";
    public bool PrettyPrint { get; set; } = true;

    public string GetOutputPrefix() =>
        $"{Plateforme}_{CodeSysteme}_{TypeDeclaration}_{CycleProduction}_{AnneeProduction}";
}
