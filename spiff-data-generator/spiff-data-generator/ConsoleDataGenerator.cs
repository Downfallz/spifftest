using Spectre.Console;
using spiff_data_generator.Common;
using spiff_data_generator.Common.Config;

namespace spiff_data_generator;

public static class ConsoleDataGenerator
{
    public static void Run()
    {
        while (true)
        {
            // 1. Welcome + type selection
            ConsoleUi.DisplayWelcome();
            var selectedTypes = ConsoleUi.PromptTypesFeuillet(Constants.TypesFeuillet);

            // 2. Load configs
            var configs = new Dictionary<string, GeneratorConfig>();
            foreach (var type in selectedTypes)
                configs[type] = GeneratorConfigLoader.Load(type);

            // 3. Action loop
            string? lastZip = null;
            string? lastOutputDir = null;

            while (true)
            {
                // Show config (single type = full detail, multi = compact table)
                if (selectedTypes.Count == 1)
                    ConsoleUi.DisplayConfig(selectedTypes[0], configs[selectedTypes[0]]);
                else
                    ConsoleUi.DisplayMultiTypePreview(selectedTypes, configs);

                var action = ConsoleUi.PromptMainMenu(selectedTypes, lastZip, lastOutputDir);

                switch (action)
                {
                    case UiActions.Generate:
                        var results = RunAllGenerations(selectedTypes, configs);
                        if (results.Count > 1)
                            ConsoleUi.ShowGrandSummary(results);
                        lastZip = results.LastOrDefault().zipPath;
                        lastOutputDir = results.LastOrDefault().outputDir;

                        var postAction = HandlePostGeneration(lastZip, lastOutputDir);
                        if (postAction == UiActions.Quit) return;
                        if (postAction == UiActions.NewSelection) goto newSelection;
                        // Regenerate or other: continue inner loop
                        continue;

                    case UiActions.OverrideParams:
                        var typeToOverride = selectedTypes.Count == 1
                            ? selectedTypes[0]
                            : ConsoleUi.PromptWhichTypeToOverride(selectedTypes);
                        ConsoleUi.PromptOverrides(configs[typeToOverride]);
                        continue;

                    case UiActions.OpenConfig:
                        ShellOpener.Open(Constants.ConfigPath);
                        continue;

                    case UiActions.OpenLastZip:
                        ShellOpener.Open(lastZip);
                        continue;

                    case UiActions.OpenOutputDir:
                        ShellOpener.Open(lastOutputDir);
                        continue;

                    case UiActions.Quit:
                        return;
                }
            }

            newSelection:;
        }
    }

    private static List<(string type, string zipPath, string outputDir, TimeSpan elapsed)> RunAllGenerations(
        List<string> types,
        Dictionary<string, GeneratorConfig> configs)
    {
        var results = new List<(string type, string zipPath, string outputDir, TimeSpan elapsed)>();

        foreach (var type in types)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[bold cyan]{Markup.Escape(type)}[/]").RuleStyle("blue"));

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var (zip, dir) = GeneratorRunner.Run(type, configs[type]);
            sw.Stop();

            results.Add((type, zip, dir, sw.Elapsed));
        }

        return results;
    }

    private static string HandlePostGeneration(string? lastZip, string? lastOutputDir)
    {
        while (true)
        {
            var action = ConsoleUi.PostGenerationMenu();

            switch (action)
            {
                case UiActions.OpenOutputDir:
                    ShellOpener.Open(lastOutputDir);
                    continue;
                case UiActions.OpenLastZip:
                    ShellOpener.Open(lastZip);
                    continue;
                case UiActions.OpenConfig:
                    ShellOpener.Open(Constants.ConfigPath);
                    continue;
                default:
                    return action; // Quit, NewSelection, Regenerate
            }
        }
    }
}
