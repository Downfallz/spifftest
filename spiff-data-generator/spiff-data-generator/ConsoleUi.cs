using Spectre.Console;
using spiff_data_generator.Common.Anomalies;
using spiff_data_generator.Common.Config;
using spiff_data_generator.Common.Export;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spiff_data_generator;
public static class ConsoleUi
{
    public static string PromptTypeFeuillet(string[] types) =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Quel type de feuillet voulez-vous générer?[/]")
                .AddChoices(types));

    public static string PromptMainMenu(string? zip, string? dir)
    {
        var menu = new SelectionPrompt<string>()
            .Title("[yellow]Que voulez-vous faire?[/]")
            .AddChoices("Générer", "Ouvrir le fichier de config", "Quitter");

        if (!string.IsNullOrEmpty(zip))
        {
            menu.AddChoice("Ouvrir le dernier fichier généré");
        }
        if (!string.IsNullOrEmpty(dir))
        {
            menu.AddChoice("Ouvrir le dossier de sortie");
        }
        return AnsiConsole.Prompt(menu);
    }

    public static bool PostGenerationMenu(string? zip, string? dir)
    {
        var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[yellow]Suite ?[/]")
            .AddChoices(
                "Relancer la génération",
                "Recharger la config",
                "Ouvrir dossier de sortie",
                "Ouvrir fichier généré," +
                "Quitter"));
        switch (choice)
        {
            case "Ouvrir dossier de sortie":
                ShellOpener.Open(dir);
                return true;
            case "Ouvrir fichier généré":
                ShellOpener.Open(zip);
                return true;
            case "Quitter":
                return false;
            default:
                return true;
        }

    }

    public static void DisplayError(Exception ex)
    {
        AnsiConsole.Write(new Panel(ex.Message).Header("[bold red]Erreur[/]").BorderStyle(Style.Parse("red")));
    }

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

    public static void AddAnomalyRow(Table table, string label, AnomalyLevelConfig level)
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

    public static string FormatWeights(int[] weights) =>
        weights.Length == 0 ? "[grey]défaut[/]" : string.Join(" / ", weights);

    public static void DisplayConfig(string typeFeuillet, GeneratorConfig config)
    {

        // ── Header ──────────────────────────────────────────
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("SPIFF Generator").Color(Color.CadetBlue));
        AnsiConsole.MarkupLine($"[grey]{typeFeuillet} — {DateTime.Now:yyyy-MM-dd HH:mm:ss}[/]");
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

        AppendAnomalies(table, config);

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

    }

    public static void AddIfPositive(Table table, string label, int value)
    {
        if (value > 0)
        {
            table.AddRow(label, value.ToString());
        }
    }

    public static void RunProgress(IZipExporter exporter, int totalLignes)
    {
        AnsiConsole.Progress()
            .AutoClear(true)
            .Columns(new TaskDescriptionColumn(),
            new ProgressBarColumn(), new PercentageColumn(), new SpinnerColumn())
            .Start(ctx =>
            {
                var task = ctx.AddTask($"[green]Génération ({totalLignes:N0} lignes)[/]",
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

    public static (string zipPath, string outputDir) ShowSummary(IZipExporter exporter, GeneratorConfig config, string requestedPrefix, System.Diagnostics.Stopwatch sw)
    {
        var actualPrefix = exporter.LastFilePrefix ?? requestedPrefix;
        var zipPath = Path.Combine(config.OutputDir, $"{actualPrefix}.zip");
        var fileSize = File.Exists(zipPath) ? new FileInfo(zipPath).Length : 0;

        var summary = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold green]Génération terminée ![/]")
            .AddColumn("[bold]Info[/]")
            .AddColumn("[bold]Valeur[/]");

        summary.AddRow("Fichier", Markup.Escape(zipPath));
        summary.AddRow("Taille", $"{fileSize:N0} bytes ({fileSize / 1024d / 1024d:F2} MB");
        summary.AddRow("Temps écoulé", $"{sw.Elapsed.TotalSeconds:F2} s]");
        summary.AddRow("Débit", $"{config.NombreLignes / sw.Elapsed.TotalSeconds:F0} lignes/sec");
        summary.AddRow("Log",
            Markup.Escape(Path.Combine(config.OutputDir, $"{actualPrefix}.log")));

        if (config.Anomalies is { Enabled: true })
        {
            var a = config.Anomalies;
            var total = a.Bloquant.Nombre + a.Importante.Nombre + a.SevereImpression.Nombre + a.Avertissement.Nombre;

            summary.AddEmptyRow();
            summary.AddRow("[red]Anomalies appliquées[/]", $"[bold]{total}[/] au total");
            AddIfPositive(summary, "  Bloquant", a.Bloquant.Nombre);
            AddIfPositive(summary, "  Importante", a.Importante.Nombre);
            AddIfPositive(summary, "  Sévère impression", a.SevereImpression.Nombre);
            AddIfPositive(summary, "  Avertissement", a.Avertissement.Nombre);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(summary);
        AnsiConsole.WriteLine();

        return (zipPath, config.OutputDir);

    }
}
