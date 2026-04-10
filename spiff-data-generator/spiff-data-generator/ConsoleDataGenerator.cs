using spiff_data_generator.Common;

namespace spiff_data_generator;
public static class ConsoleDataGenerator
{
    public static void Run()
    {
        string? lastZip = null;
        string? lastOutputDir = null;

        var typeFeuillet = ConsoleUi.PromptTypeFeuillet(Common.Constants.TypesFeuillet);

        while (true)
        {
            var config = GeneratorConfigLoader.Load(typeFeuillet);

            ConsoleUi.DisplayConfig(typeFeuillet, config);

            var action = ConsoleUi.PromptMainMenu(lastZip, lastOutputDir);

            switch (action)
            {
                case UiActions.Generate:
                    (lastZip, lastOutputDir) = GeneratorRunner.Run(typeFeuillet, config);
                    break;
                case UiActions.OpenConfig:
                    ShellOpener.Open(Constants.ConfigPath);
                    break;

                case UiActions.OpenLastZip:
                    ShellOpener.Open(lastZip);
                    break;

                case UiActions.OpenOutputDir:
                    ShellOpener.Open(lastOutputDir);
                    break;

                case UiActions.Quit:
                    return;
            }

            if (!ConsoleUi.PostGenerationMenu(lastZip, lastOutputDir))
            {
                break;
            }
        }
    }
}
