using System.Globalization;
using Bogus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.Common.Export;
using spiff_data_generator.Common.Interfaces;
using spiff_data_generator.Common.Logging;
using spiff_data_generator.Common.RandomGen;
using spiff_data_generator.T5Rl3.Builders;
using spiff_data_generator.T5Rl3.Config;
using spiff_data_generator.T5Rl3.Generation;

while (true)
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
        .Build();

    var config = configuration.GetSection("T5Rl3").Get<T5Rl3Config>() ?? new T5Rl3Config();
    Randomizer.Seed = new Random(config.Seed);

    // ── Header ──────────────────────────────────────────
    AnsiConsole.Clear();
    AnsiConsole.Write(new FigletText("SPIFF Generator").Color(Color.CadetBlue));
    AnsiConsole.MarkupLine($"[grey]T5RL3 / T5 — {DateTime.Now:yyyy-MM-dd HH:mm:ss}[/]");
    AnsiConsole.WriteLine();

    // ── Config table ────────────────────────────────────
    var table = new Table()
        .Border(TableBorder.Rounded)
        .Title("[bold yellow]Configuration[/]")
        .AddColumn("[bold]Paramètre[/]")
        .AddColumn("[bold]Valeur[/]");

    table.AddRow("Plateforme", config.Plateforme);
    table.AddRow("Code système", config.CodeSysteme);
    table.AddRow("Type déclaration", config.TypeDeclaration);
    table.AddRow("Cycle production", config.CycleProduction);
    table.AddRow("Année production", config.AnneeProduction);
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
    table.AddRow("Devises", string.Join(", ", config.Devises));
    table.AddEmptyRow();
    table.AddRow("Output", Markup.Escape(config.OutputDir));
    table.AddRow("Pretty print", config.PrettyPrint ? "[green]Oui[/]" : "Non");

    // Anomalies section
    if (config.Anomalies.Enabled)
    {
        table.AddEmptyRow();
        table.AddRow("[red]Anomalies[/]", "[green]Activées[/]");
        AddAnomalyRow(table, "Bloquant", config.Anomalies.Bloquant);
        AddAnomalyRow(table, "Importante", config.Anomalies.Importante);
        AddAnomalyRow(table, "Sévère impression", config.Anomalies.SevereImpression);
        AddAnomalyRow(table, "Avertissement", config.Anomalies.Avertissement);

        int totalAnomalies = config.Anomalies.Bloquant.Nombre
            + config.Anomalies.Importante.Nombre
            + config.Anomalies.SevereImpression.Nombre
            + config.Anomalies.Avertissement.Nombre;
        table.AddRow("[bold red]Total anomalies[/]", $"[bold]{totalAnomalies}[/]");
    }
    else
    {
        table.AddEmptyRow();
        table.AddRow("[red]Anomalies[/]", "[grey]Désactivées[/]");
    }

    AnsiConsole.Write(table);
    AnsiConsole.WriteLine();

    // ── Menu ──────────────────────────────────────────────
    var action = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[yellow]Que voulez-vous faire?[/]")
            .AddChoices("Lancer la génération", "Recharger appsettings", "Quitter"));

    if (action == "Quitter") break;
    if (action == "Recharger appsettings") continue;

    // ── Generate ────────────────────────────────────────
    // Resolve next available file prefix (auto-increment 01, 02, ...)
    var currentDate = DateTime.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
    var filePrefix = config.GetNextFilePrefix(currentDate);
    using var logger = new FileGenerationLogger(config.OutputDir, filePrefix);

    var services = new ServiceCollection()
        .AddSingleton(config)
        .AddSingleton<IGenerationLogger>(logger)
        .AddSingleton<IRandomService, RandomService>()
        .AddSingleton<ISlipBuilder, IndividuSlipBuilder>()
        .AddSingleton<ISlipBuilder, OrganisationSlipBuilder>()
        .AddSingleton<IAnomalyService, AnomalyService>()
        .AddSingleton<ISlipGenerator, SlipGenerator>()
        .AddSingleton<IZipExporter, ZipExporter>()
        .BuildServiceProvider();

    var exporter = services.GetRequiredService<IZipExporter>();

    var sw = System.Diagnostics.Stopwatch.StartNew();
    Exception? genError = null;

    try
    {
        AnsiConsole.Progress()
            .AutoClear(true)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn())
            .Start(ctx =>
            {
                var task = ctx.AddTask($"[green]Génération ({config.NombreLignes:N0} lignes)[/]",
                    maxValue: config.NombreLignes);
                exporter.OnProgress = (current, _) => task.Value = current;
                exporter.ExportToFile();
                task.Value = config.NombreLignes;
            });
    }
    catch (Exception ex)
    {
        genError = ex;
    }

    sw.Stop();

    if (genError != null)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Panel(
            new Text(genError.Message, new Style(Color.White))
            )
            .Header("[bold red]Erreur lors de la génération[/]")
            .Border(BoxBorder.Heavy)
            .BorderColor(Color.Red));
        AnsiConsole.MarkupLine($"[grey]{Markup.Escape(genError.GetType().Name)}[/]");
        if (genError.InnerException != null)
            AnsiConsole.MarkupLine($"[grey]  → {Markup.Escape(genError.InnerException.Message)}[/]");
        AnsiConsole.WriteLine();
        services.Dispose();
        continue;
    }

    // ── Summary ─────────────────────────────────────────
    AnsiConsole.WriteLine();
    var actualPrefix = exporter.LastFilePrefix ?? filePrefix;
    var zipPath = Path.Combine(config.OutputDir, $"{actualPrefix}.zip");
    long fileSize = File.Exists(zipPath) ? new FileInfo(zipPath).Length : 0;

    var summary = new Table()
        .Border(TableBorder.Rounded)
        .Title("[bold green]Génération terminée[/]")
        .AddColumn("[bold]Info[/]")
        .AddColumn("[bold]Valeur[/]");

    summary.AddRow("Fichier", Markup.Escape(zipPath));
    summary.AddRow("Taille", $"{fileSize:N0} bytes ({fileSize / 1024.0 / 1024.0:F2} MB)");
    summary.AddRow("Durée", $"{sw.Elapsed.TotalSeconds:F2}s");
    summary.AddRow("Débit", $"{config.NombreLignes / sw.Elapsed.TotalSeconds:F0} lignes/sec");
    summary.AddRow("Log", Markup.Escape(Path.Combine(config.OutputDir, $"{actualPrefix}.log")));

    if (config.Anomalies.Enabled)
    {
        summary.AddEmptyRow();
        AddAnomalySummaryRow(summary, "Bloquant", config.Anomalies.Bloquant);
        AddAnomalySummaryRow(summary, "Importante", config.Anomalies.Importante);
        AddAnomalySummaryRow(summary, "Sévère impression", config.Anomalies.SevereImpression);
        AddAnomalySummaryRow(summary, "Avertissement", config.Anomalies.Avertissement);
    }

    AnsiConsole.Write(summary);
    AnsiConsole.WriteLine();

    services.Dispose();

    // ── Post-generation menu ────────────────────────────
    var next = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[yellow]Que voulez-vous faire?[/]")
            .AddChoices("Relancer la génération", "Recharger appsettings", "Quitter"));

    if (next == "Quitter") break;
    // Both "Relancer" and "Recharger" loop back — appsettings is always reloaded at top
}

AnsiConsole.MarkupLine("[grey]Au revoir![/]");

// ── Local helpers ───────────────────────────────────────

static string FormatWeights(int[] weights) =>
    weights.Length == 0 ? "[grey]défaut[/]" : string.Join(" / ", weights);

static void AddAnomalyRow(Table table, string label, AnomalyLevelConfig level)
{
    if (level.Nombre > 0)
    {
        var types = string.Join(", ", level.Types.Select(t => t.ToString()));
        table.AddRow($"  {label}", $"{level.Nombre}x — {Markup.Escape(types)}");
    }
    else
    {
        table.AddRow($"  {label}", "[grey]0[/]");
    }
}

static void AddAnomalySummaryRow(Table table, string label, AnomalyLevelConfig level)
{
    if (level.Nombre > 0)
        table.AddRow($"[red]Anomalies {label}[/]", $"{level.Nombre} appliquées");
}
