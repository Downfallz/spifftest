using Spectre.Console;
using spiff_data_generator.Common;
using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.Common.Config;
using spiff_data_generator.Common.Export;

namespace spiff_data_generator;

public static class ConsoleUi
{
    // ── Welcome & Navigation ────────────────────────────────

    public static void DisplayWelcome()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("SPIFF Generator").Color(Color.CadetBlue));
        AnsiConsole.Write(new Rule("[grey]Générateur de données fiscales[/]").RuleStyle("blue"));
        AnsiConsole.WriteLine();
    }

    public static List<string> PromptTypesFeuillet(string[] types)
    {
        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[yellow]Quels types de feuillets générer?[/]")
                .PageSize(10)
                .InstructionsText("[grey](Espace = sélectionner, Entrée = confirmer)[/]")
                .AddChoices(types)
                .Required());
    }

    public static string PromptMainMenu(List<string> selectedTypes, string? zip, string? dir)
    {
        var typesLabel = string.Join(", ", selectedTypes);
        AnsiConsole.Write(new Rule($"[bold cyan]{Markup.Escape(typesLabel)}[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();

        var choices = new List<string>
        {
            UiActions.Generate,
            UiActions.OverrideParams,
            UiActions.OpenConfig,
        };
        if (!string.IsNullOrEmpty(zip)) choices.Add(UiActions.OpenLastZip);
        if (!string.IsNullOrEmpty(dir)) choices.Add(UiActions.OpenOutputDir);
        choices.Add(UiActions.Quit);

        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Que voulez-vous faire?[/]")
                .AddChoices(choices));
    }

    public static string PostGenerationMenu()
    {
        AnsiConsole.WriteLine();
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Suite ?[/]")
                .AddChoices(
                    UiActions.NewSelection,
                    UiActions.Regenerate,
                    UiActions.OpenOutputDir,
                    UiActions.OpenLastZip,
                    UiActions.OpenConfig,
                    UiActions.Quit));
    }

    // ── Config Display ──────────────────────────────────────

    public static void DisplayConfig(string typeFeuillet, GeneratorConfig config)
    {
        AnsiConsole.Write(new Rule($"[bold yellow]Configuration — {Markup.Escape(typeFeuillet)}[/]").RuleStyle("yellow"));
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Paramètre[/]")
            .AddColumn("[bold]Valeur[/]");

        table.AddRow("Plateforme", Markup.Escape(config.Plateforme));
        table.AddRow("Code système", Markup.Escape(config.CodeSysteme));
        table.AddRow("Type déclaration", Markup.Escape(config.TypeDeclaration));
        table.AddRow("Cycle production", Markup.Escape(config.CycleProduction));
        table.AddRow("Année production", Markup.Escape(config.AnneeProduction));
        table.AddEmptyRow();
        table.AddRow("Seed", config.Seed.ToString());
        table.AddRow("[green]Individus[/]", $"{config.NombreIndividus:N0}");
        table.AddRow("[blue]Organisations[/]", $"{config.NombreLignes - config.NombreIndividus:N0}");
        table.AddRow("[bold]Total lignes[/]", $"[bold]{config.NombreLignes:N0}[/]");
        table.AddRow("Batch size", $"{config.BatchSize:N0}");
        table.AddEmptyRow();
        table.AddRow("Weights province (QC/Autre)", FormatWeights(config.WeightsCodeProvince));
        table.AddRow("Weights impression (PN/N)", FormatWeights(config.WeightsImpression));
        table.AddRow("Weights courrier retenu", FormatWeights(config.WeightsCourrierRetenu));
        table.AddRow("Indicateur Ontario", config.IndicateurOntario ? "[green]Oui[/]" : "Non");
        table.AddRow("Feuillets / caisse", $"{config.NombreFeuilletParCaisse:N0}");
        table.AddEmptyRow();
        table.AddRow("Émetteur fourni", config.AjouterEmetteurFourni ? "[green]Oui[/]" : "Non");
        table.AddRow("ID unique", config.AjouterIdUnique ? $"[green]Oui[/] (prefix: {Markup.Escape(config.PrefixeIdentificationUnique)})" : "Non");
        table.AddRow("Devises", Markup.Escape(string.Join(", ", config.Devises)));
        table.AddEmptyRow();
        table.AddRow("Output", Markup.Escape(config.OutputDir));
        table.AddRow("Pretty print", config.PrettyPrint ? "[green]Oui[/]" : "Non");

        AppendAnomalies(table, config);

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    public static void DisplayMultiTypePreview(List<string> types, Dictionary<string, GeneratorConfig> configs)
    {
        AnsiConsole.Write(new Rule("[bold yellow]Feuillets sélectionnés[/]").RuleStyle("yellow"));
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Type[/]")
            .AddColumn("[bold]Lignes[/]")
            .AddColumn("[bold]Individus[/]")
            .AddColumn("[bold]Année[/]")
            .AddColumn("[bold]Code sys[/]")
            .AddColumn("[bold]Décl.[/]")
            .AddColumn("[bold]Output[/]");

        foreach (var type in types)
        {
            var c = configs[type];
            table.AddRow(
                $"[cyan]{Markup.Escape(type)}[/]",
                $"{c.NombreLignes:N0}",
                $"{c.NombreIndividus:N0}",
                c.AnneeProduction,
                c.CodeSysteme,
                c.TypeDeclaration,
                Markup.Escape(c.OutputDir));
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    // ── Inline Overrides ────────────────────────────────────

    public static string PromptWhichTypeToOverride(List<string> types)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Quel type modifier?[/]")
                .AddChoices(types));
    }

    public static void PromptOverrides(GeneratorConfig config)
    {
        while (true)
        {
            DisplayQuickConfig(config);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Quel paramètre modifier?[/]")
                    .AddChoices(
                        UiActions.OverrideNombreLignes,
                        UiActions.OverrideNombreIndividus,
                        UiActions.OverrideBatchSize,
                        UiActions.OverrideCodeSysteme,
                        UiActions.OverrideAnneeProduction,
                        UiActions.OverrideTypeDeclaration,
                        UiActions.OverrideDone));

            if (choice == UiActions.OverrideDone) break;

            ApplyOverride(config, choice);

            // Auto-fix: NombreIndividus cannot exceed NombreLignes
            if (config.NombreIndividus > config.NombreLignes)
            {
                config.NombreIndividus = config.NombreLignes;
                AnsiConsole.MarkupLine("[grey]NombreIndividus ajusté à NombreLignes[/]");
            }
        }
    }

    private static void ApplyOverride(GeneratorConfig config, string param)
    {
        switch (param)
        {
            case UiActions.OverrideNombreLignes:
                config.NombreLignes = AnsiConsole.Prompt(
                    new TextPrompt<int>($"[green]Nombre de lignes[/] [grey]({config.NombreLignes:N0})[/]:")
                        .DefaultValue(config.NombreLignes)
                        .Validate(v => v > 0 ? ValidationResult.Success() : ValidationResult.Error("Doit être > 0")));
                break;

            case UiActions.OverrideNombreIndividus:
                config.NombreIndividus = AnsiConsole.Prompt(
                    new TextPrompt<int>($"[green]Nombre d'individus[/] [grey]({config.NombreIndividus:N0})[/]:")
                        .DefaultValue(config.NombreIndividus)
                        .Validate(v => v > 0 ? ValidationResult.Success() : ValidationResult.Error("Doit être > 0")));
                break;

            case UiActions.OverrideBatchSize:
                config.BatchSize = AnsiConsole.Prompt(
                    new TextPrompt<int>($"[green]Batch size[/] [grey]({config.BatchSize:N0})[/]:")
                        .DefaultValue(config.BatchSize)
                        .Validate(v => v > 0 ? ValidationResult.Success() : ValidationResult.Error("Doit être > 0")));
                break;

            case UiActions.OverrideCodeSysteme:
                config.CodeSysteme = AnsiConsole.Prompt(
                    new TextPrompt<string>($"[green]Code système[/] [grey]({Markup.Escape(config.CodeSysteme)})[/]:")
                        .DefaultValue(config.CodeSysteme));
                break;

            case UiActions.OverrideAnneeProduction:
                config.AnneeProduction = AnsiConsole.Prompt(
                    new TextPrompt<string>($"[green]Année de production[/] [grey]({config.AnneeProduction})[/]:")
                        .DefaultValue(config.AnneeProduction)
                        .Validate(v => v.Length == 4 && int.TryParse(v, out _)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("Format YYYY requis")));
                break;

            case UiActions.OverrideTypeDeclaration:
                config.TypeDeclaration = AnsiConsole.Prompt(
                    new TextPrompt<string>($"[green]Type déclaration[/] [grey]({config.TypeDeclaration})[/]:")
                        .DefaultValue(config.TypeDeclaration)
                        .Validate(v => v is "O" or "A"
                            ? ValidationResult.Success()
                            : ValidationResult.Error("O (Originale) ou A (Amendée)")));
                break;
        }
    }

    private static void DisplayQuickConfig(GeneratorConfig config)
    {
        AnsiConsole.WriteLine();
        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn(new GridColumn());
        grid.AddRow("[bold]Lignes:[/]", $"{config.NombreLignes:N0}");
        grid.AddRow("[bold]Individus:[/]", $"{config.NombreIndividus:N0}");
        grid.AddRow("[bold]Batch:[/]", $"{config.BatchSize:N0}");
        grid.AddRow("[bold]Code sys:[/]", config.CodeSysteme);
        grid.AddRow("[bold]Année:[/]", config.AnneeProduction);
        grid.AddRow("[bold]Décl.:[/]", config.TypeDeclaration);

        AnsiConsole.Write(new Panel(grid)
            .Header("[bold cyan]Paramètres clés[/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Style.Parse("cyan")));
    }

    // ── Generation Progress & Summary ───────────────────────

    public static void RunProgress(IZipExporter exporter, int totalLignes, string typeFeuillet)
    {
        AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn())
            .Start(ctx =>
            {
                var task = ctx.AddTask(
                    $"[green]{Markup.Escape(typeFeuillet)}[/] ({totalLignes:N0} lignes)",
                    maxValue: totalLignes);

                exporter.OnProgress = (current, total) =>
                {
                    task.Value = current;
                    task.MaxValue = total;
                };
                exporter.ExportToFile();
                task.Value = totalLignes;
            });
    }

    public static (string zipPath, string outputDir) ShowSummary(
        IZipExporter exporter, GeneratorConfig config, string requestedPrefix, System.Diagnostics.Stopwatch sw)
    {
        var actualPrefix = exporter.LastFilePrefix ?? requestedPrefix;
        var zipPath = Path.Combine(config.OutputDir, $"{actualPrefix}.zip");
        var fileSize = File.Exists(zipPath) ? new FileInfo(zipPath).Length : 0;

        var summary = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold green]Génération terminée[/]")
            .AddColumn("[bold]Info[/]")
            .AddColumn("[bold]Valeur[/]");

        summary.AddRow("Fichier", Markup.Escape(zipPath));
        summary.AddRow("Taille", $"{fileSize:N0} bytes ({fileSize / 1024d / 1024d:F2} MB)");
        summary.AddRow("Temps", $"{sw.Elapsed.TotalSeconds:F2} s");
        summary.AddRow("Débit", $"{config.NombreLignes / sw.Elapsed.TotalSeconds:F0} lignes/sec");

        if (config.Anomalies is { Enabled: true })
        {
            var a = config.Anomalies;
            var total = a.Bloquant.Nombre + a.Importante.Nombre + a.SevereImpression.Nombre + a.Avertissement.Nombre;
            summary.AddEmptyRow();
            summary.AddRow("[red]Anomalies[/]", $"[bold]{total}[/] appliquées");
        }

        AnsiConsole.Write(summary);
        AnsiConsole.WriteLine();

        return (zipPath, config.OutputDir);
    }

    public static void ShowGrandSummary(List<(string type, string zipPath, string outputDir, TimeSpan elapsed)> results)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold green]Résumé global[/]").RuleStyle("green"));
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Type[/]")
            .AddColumn("[bold]Fichier[/]")
            .AddColumn("[bold]Taille[/]")
            .AddColumn("[bold]Temps[/]");

        foreach (var (type, zip, _, elapsed) in results)
        {
            var fileName = Path.GetFileName(zip);
            var size = !string.IsNullOrEmpty(zip) && File.Exists(zip) ? new FileInfo(zip).Length : 0;
            table.AddRow(
                $"[cyan]{Markup.Escape(type)}[/]",
                string.IsNullOrEmpty(fileName) ? "[red]Erreur[/]" : Markup.Escape(fileName),
                size > 0 ? $"{size / 1024d / 1024d:F2} MB" : "[grey]-[/]",
                $"{elapsed.TotalSeconds:F1}s");
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    // ── Errors ──────────────────────────────────────────────

    public static void DisplayError(Exception ex)
    {
        AnsiConsole.Write(new Panel(Markup.Escape(ex.Message))
            .Header("[bold red]Erreur[/]")
            .BorderStyle(Style.Parse("red")));
    }

    // ── Helpers ─────────────────────────────────────────────

    public static void AppendAnomalies(Table table, GeneratorConfig config)
    {
        table.AddEmptyRow();

        if (config.Anomalies is not { Enabled: true })
        {
            table.AddRow("[red]Anomalies[/]", "[grey]Désactivées[/]");
            return;
        }

        table.AddRow("[red]Anomalies[/]", "[green]Activées[/]");
        AddAnomalyRow(table, "Bloquant", config.Anomalies.Bloquant);
        AddAnomalyRow(table, "Importante", config.Anomalies.Importante);
        AddAnomalyRow(table, "Sévère impression", config.Anomalies.SevereImpression);
        AddAnomalyRow(table, "Avertissement", config.Anomalies.Avertissement);
    }

    private static void AddAnomalyRow(Table table, string label, AnomalyLevelConfig level)
    {
        if (level.Nombre > 0)
        {
            var types = string.Join(", ", level.Types);
            table.AddRow($"  {label}", $"{level.Nombre}x — {Markup.Escape(types)}");
        }
        else
        {
            table.AddRow($"  {label}", "[grey]0[/]");
        }
    }

    private static string FormatWeights(int[] weights) =>
        weights.Length == 0 ? "[grey]défaut[/]" : string.Join(" / ", weights);
}
