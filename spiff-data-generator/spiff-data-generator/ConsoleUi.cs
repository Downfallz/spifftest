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
        AnsiConsole.Write(new Rule($"[grey]Générateur de données fiscales — {DateTime.Now:yyyy-MM-dd HH:mm}[/]")
            .RuleStyle("blue"));
        AnsiConsole.WriteLine();
    }

    public static List<string> PromptTypesFeuillet(string[] types)
    {
        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[yellow]Quels types de feuillets générer?[/]")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Cyan1))
                .InstructionsText("[grey](Espace = sélectionner, Entrée = confirmer)[/]")
                .AddChoices(types)
                .Required());
    }

    public static string PromptMainMenu(List<string> selectedTypes, string? zip, string? dir)
    {
        var typesLabel = string.Join("  ·  ", selectedTypes);
        AnsiConsole.Write(new Rule($"[bold cyan]{Markup.Escape(typesLabel)}[/]").RuleStyle("blue"));
        AnsiConsole.WriteLine();

        var choices = new List<string>
        {
            UiActions.Generate,
            UiActions.OverrideParams,
            UiActions.OpenConfig,
            UiActions.ReloadConfig,
        };
        if (!string.IsNullOrEmpty(zip))
        {
            choices.Add(UiActions.OpenLastZip);
            choices.Add(UiActions.OpenLog);
        }
        if (!string.IsNullOrEmpty(dir)) choices.Add(UiActions.OpenOutputDir);
        choices.Add(UiActions.Quit);

        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Que voulez-vous faire?[/]")
                .HighlightStyle(new Style(Color.Cyan1))
                .AddChoices(choices));
    }

    public static string PostGenerationMenu()
    {
        AnsiConsole.WriteLine();
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Suite ?[/]")
                .HighlightStyle(new Style(Color.Cyan1))
                .AddChoices(
                    UiActions.Regenerate,
                    UiActions.NewSelection,
                    UiActions.OpenOutputDir,
                    UiActions.OpenLastZip,
                    UiActions.OpenLog,
                    UiActions.OpenConfig,
                    UiActions.ReloadConfig,
                    UiActions.Quit));
    }

    // ── Config Display ──────────────────────────────────────

    public static void DisplayConfig(string typeFeuillet, GeneratorConfig config)
    {
        AnsiConsole.Write(new Rule($"[bold yellow]Configuration — {Markup.Escape(typeFeuillet)}[/]")
            .RuleStyle("yellow"));
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderStyle(Style.Parse("grey"))
            .AddColumn(new TableColumn("[bold]Paramètre[/]").PadRight(1))
            .AddColumn("[bold]Valeur[/]");

        // Identifiants
        table.AddRow("[dim]Plateforme[/]", Markup.Escape(config.Plateforme));
        table.AddRow("Code système", $"[bold]{Markup.Escape(config.CodeSysteme)}[/]");
        table.AddRow("Type déclaration", $"[bold]{Markup.Escape(config.TypeDeclaration)}[/]");
        table.AddRow("[dim]Cycle production[/]", Markup.Escape(config.CycleProduction));
        table.AddRow("Année production", $"[bold]{Markup.Escape(config.AnneeProduction)}[/]");
        table.AddEmptyRow();

        // Volumes
        table.AddRow("[dim]Seed[/]", config.Seed == 0 ? "[grey]aléatoire[/]" : config.Seed.ToString());
        table.AddRow("[green]Individus[/]", $"[bold]{config.NombreIndividus:N0}[/]");
        table.AddRow("[blue]Organisations[/]", $"[bold]{config.NombreLignes - config.NombreIndividus:N0}[/]");
        table.AddRow("[bold]Total lignes[/]", $"[bold yellow]{config.NombreLignes:N0}[/]");
        table.AddRow("Batch size", $"{config.BatchSize:N0}");
        table.AddEmptyRow();

        // Weights
        table.AddRow("[dim]Province (QC/Autre)[/]", FormatWeights(config.WeightsCodeProvince));
        table.AddRow("[dim]Impression (PN/N)[/]", FormatWeights(config.WeightsImpression));
        table.AddRow("[dim]Courrier retenu[/]", FormatWeights(config.WeightsCourrierRetenu));
        table.AddRow("[dim]Ontario seulement[/]", config.IndicateurOntario ? "[green]Oui[/]" : "[grey]Non[/]");
        table.AddRow("[dim]Feuillets / caisse[/]", $"{config.NombreFeuilletParCaisse:N0}");
        table.AddEmptyRow();

        // Options
        table.AddRow("[dim]Émetteur fourni[/]", config.AjouterEmetteurFourni ? "[green]Oui[/]" : "[grey]Non[/]");
        table.AddRow("[dim]ID unique[/]", config.AjouterIdUnique
            ? $"[green]Oui[/] [grey]prefix: {Markup.Escape(config.PrefixeIdentificationUnique)}[/]"
            : "[grey]Non[/]");
        table.AddRow("[dim]Devises[/]", Markup.Escape(string.Join(", ", config.Devises)));
        table.AddRow("[dim]Pretty print[/]", config.PrettyPrint ? "[green]Oui[/]" : "[grey]Non[/]");
        table.AddRow("Output", $"[blue]{Markup.Escape(config.OutputDir)}[/]");

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
            .BorderStyle(Style.Parse("grey"))
            .AddColumn("[bold]Type[/]")
            .AddColumn(new TableColumn("[bold]Lignes[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Individus[/]").RightAligned())
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
                $"[blue]{Markup.Escape(c.OutputDir)}[/]");
        }

        // Total row
        var totalLignes = types.Sum(t => configs[t].NombreLignes);
        var totalIndividus = types.Sum(t => configs[t].NombreIndividus);
        table.AddEmptyRow();
        table.AddRow(
            $"[bold]{types.Count} types[/]",
            $"[bold yellow]{totalLignes:N0}[/]",
            $"[bold]{totalIndividus:N0}[/]",
            "", "", "", "");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    // ── Pre-Generation Confirmation ─────────────────────────

    public static bool ConfirmGeneration(List<string> types, Dictionary<string, GeneratorConfig> configs)
    {
        var totalLignes = types.Sum(t => configs[t].NombreLignes);

        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn(new GridColumn());
        grid.AddRow("[bold]Types:[/]", $"[cyan]{Markup.Escape(string.Join(", ", types))}[/]");
        grid.AddRow("[bold]Total lignes:[/]", $"[yellow]{totalLignes:N0}[/]");

        var firstConfig = configs[types[0]];
        grid.AddRow("[bold]Code sys:[/]", firstConfig.CodeSysteme);
        grid.AddRow("[bold]Année:[/]", firstConfig.AnneeProduction);

        AnsiConsole.Write(new Panel(grid)
            .Header("[bold green]Lancer la génération ?[/]")
            .Border(BoxBorder.Double)
            .BorderStyle(Style.Parse("green")));

        return AnsiConsole.Confirm("[green]Confirmer[/]?", defaultValue: true);
    }

    // ── Inline Overrides ────────────────────────────────────

    public static string PromptWhichTypeToOverride(List<string> types)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Quel type modifier?[/]")
                .HighlightStyle(new Style(Color.Cyan1))
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
                    .HighlightStyle(new Style(Color.Cyan1))
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

            if (config.NombreIndividus > config.NombreLignes)
            {
                config.NombreIndividus = config.NombreLignes;
                AnsiConsole.MarkupLine("[yellow]NombreIndividus ajusté à NombreLignes[/]");
            }
        }
    }

    private static void ApplyOverride(GeneratorConfig config, string param)
    {
        switch (param)
        {
            case UiActions.OverrideNombreLignes:
                config.NombreLignes = PromptInt("Nombre de lignes", config.NombreLignes);
                break;
            case UiActions.OverrideNombreIndividus:
                config.NombreIndividus = PromptInt("Nombre d'individus", config.NombreIndividus);
                break;
            case UiActions.OverrideBatchSize:
                config.BatchSize = PromptInt("Batch size", config.BatchSize);
                break;
            case UiActions.OverrideCodeSysteme:
                config.CodeSysteme = PromptString("Code système", config.CodeSysteme);
                break;
            case UiActions.OverrideAnneeProduction:
                config.AnneeProduction = AnsiConsole.Prompt(
                    new TextPrompt<string>($"  [green]Année[/] [grey]({config.AnneeProduction})[/]:")
                        .DefaultValue(config.AnneeProduction)
                        .Validate(v => v.Length == 4 && int.TryParse(v, out _)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Format YYYY requis[/]")));
                break;
            case UiActions.OverrideTypeDeclaration:
                config.TypeDeclaration = AnsiConsole.Prompt(
                    new TextPrompt<string>($"  [green]Déclaration[/] [grey]({config.TypeDeclaration})[/]:")
                        .DefaultValue(config.TypeDeclaration)
                        .Validate(v => v is "O" or "A"
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]O (Originale) ou A (Amendée)[/]")));
                break;
        }
    }

    private static int PromptInt(string label, int current) =>
        AnsiConsole.Prompt(
            new TextPrompt<int>($"  [green]{label}[/] [grey]({current:N0})[/]:")
                .DefaultValue(current)
                .Validate(v => v > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Doit être > 0[/]")));

    private static string PromptString(string label, string current) =>
        AnsiConsole.Prompt(
            new TextPrompt<string>($"  [green]{label}[/] [grey]({Markup.Escape(current)})[/]:")
                .DefaultValue(current));

    private static void DisplayQuickConfig(GeneratorConfig config)
    {
        AnsiConsole.WriteLine();
        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn(new GridColumn());
        grid.AddRow("[bold]Lignes:[/]", $"[yellow]{config.NombreLignes:N0}[/]");
        grid.AddRow("[bold]Individus:[/]", $"[green]{config.NombreIndividus:N0}[/]");
        grid.AddRow("[bold]Orgs:[/]", $"[blue]{config.NombreLignes - config.NombreIndividus:N0}[/]");
        grid.AddRow("[bold]Batch:[/]", $"{config.BatchSize:N0}");
        grid.AddRow("[bold]Code sys:[/]", config.CodeSysteme);
        grid.AddRow("[bold]Année:[/]", config.AnneeProduction);
        grid.AddRow("[bold]Décl.:[/]", config.TypeDeclaration);

        AnsiConsole.Write(new Panel(grid)
            .Header("[bold cyan]Paramètres[/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Style.Parse("cyan"))
            .Padding(1, 0));
    }

    // ── Generation Progress & Summary ───────────────────────

    public static void RunProgress(IZipExporter exporter, int totalLignes, string typeFeuillet)
    {
        AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn() { CompletedStyle = new Style(Color.Green) },
                new PercentageColumn(),
                new ElapsedTimeColumn(),
                new SpinnerColumn())
            .Start(ctx =>
            {
                var task = ctx.AddTask(
                    $"[cyan]{Markup.Escape(typeFeuillet)}[/] [grey]({totalLignes:N0} lignes)[/]",
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
        var logPath = Path.Combine(config.OutputDir, $"{actualPrefix}.log");
        var fileSize = File.Exists(zipPath) ? new FileInfo(zipPath).Length : 0;
        var throughput = config.NombreLignes / sw.Elapsed.TotalSeconds;
        int batchCount = (int)Math.Ceiling((double)config.NombreLignes / config.BatchSize);

        // Tree view of generated files
        var tree = new Tree($"[blue]{Markup.Escape(config.OutputDir)}[/]")
            .Style("dim");
        var zipNode = tree.AddNode($"[green]{Markup.Escape(Path.GetFileName(zipPath))}[/]  [grey]{FormatSize(fileSize)}[/]");
        zipNode.AddNode($"[grey]{batchCount} fichier(s) JSON[/]");
        tree.AddNode($"[dim]{Markup.Escape(Path.GetFileName(logPath))}[/]");

        AnsiConsole.Write(tree);

        // Metrics
        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn(new GridColumn());
        grid.AddRow("[bold]Temps:[/]", $"{sw.Elapsed.TotalSeconds:F2}s");
        grid.AddRow("[bold]Débit:[/]", $"[green]{throughput:F0} lignes/sec[/]");
        grid.AddRow("[bold]Taille:[/]", FormatSize(fileSize));

        if (config.Anomalies is { Enabled: true })
        {
            var a = config.Anomalies;
            var total = a.Bloquant.Nombre + a.Importante.Nombre + a.SevereImpression.Nombre + a.Avertissement.Nombre;
            grid.AddRow("[bold]Anomalies:[/]", $"[red]{total}[/] appliquées");
        }

        AnsiConsole.Write(new Panel(grid)
            .Border(BoxBorder.Rounded)
            .BorderStyle(Style.Parse("green"))
            .Padding(1, 0));
        AnsiConsole.WriteLine();

        return (zipPath, config.OutputDir);
    }

    public static void ShowGrandSummary(
        List<(string type, string zipPath, string outputDir, TimeSpan elapsed)> results)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold green]Résumé[/]").RuleStyle("green"));
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Heavy)
            .BorderStyle(Style.Parse("green"))
            .AddColumn("[bold]Type[/]")
            .AddColumn("[bold]Fichier[/]")
            .AddColumn(new TableColumn("[bold]Taille[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Temps[/]").RightAligned());

        long totalSize = 0;
        var totalTime = TimeSpan.Zero;

        foreach (var (type, zip, _, elapsed) in results)
        {
            var fileName = Path.GetFileName(zip);
            long size = !string.IsNullOrEmpty(zip) && File.Exists(zip) ? new FileInfo(zip).Length : 0;
            totalSize += size;
            totalTime += elapsed;

            table.AddRow(
                $"[cyan]{Markup.Escape(type)}[/]",
                string.IsNullOrEmpty(fileName) ? "[red]Erreur[/]" : Markup.Escape(fileName),
                size > 0 ? FormatSize(size) : "[grey]-[/]",
                $"{elapsed.TotalSeconds:F1}s");
        }

        // Totals
        table.AddEmptyRow();
        table.AddRow(
            $"[bold]{results.Count} types[/]",
            "",
            $"[bold]{FormatSize(totalSize)}[/]",
            $"[bold]{totalTime.TotalSeconds:F1}s[/]");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    // ── Errors ──────────────────────────────────────────────

    public static void DisplayError(Exception ex)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Panel(Markup.Escape(ex.Message))
            .Header("[bold red]Erreur[/]")
            .Border(BoxBorder.Heavy)
            .BorderStyle(Style.Parse("red")));
    }

    // ── Helpers ─────────────────────────────────────────────

    private static string FormatSize(long bytes) => bytes switch
    {
        >= 1024 * 1024 => $"{bytes / 1024d / 1024d:F2} MB",
        >= 1024 => $"{bytes / 1024d:F1} KB",
        _ => $"{bytes} B"
    };

    private static void AppendAnomalies(Table table, GeneratorConfig config)
    {
        table.AddEmptyRow();

        if (config.Anomalies is not { Enabled: true })
        {
            table.AddRow("[dim]Anomalies[/]", "[grey]Désactivées[/]");
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
            table.AddRow($"  [dim]{label}[/]", $"[red]{level.Nombre}x[/] — [grey]{Markup.Escape(types)}[/]");
        }
        else
        {
            table.AddRow($"  [dim]{label}[/]", "[grey]0[/]");
        }
    }

    private static string FormatWeights(int[] weights) =>
        weights.Length == 0 ? "[grey]défaut[/]" : $"[dim]{string.Join(" / ", weights)}[/]";
}
