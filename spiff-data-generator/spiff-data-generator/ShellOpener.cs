using Spectre.Console;

namespace spiff_data_generator;
public static class ShellOpener
{
    public static void Open(string? path)
    {
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(path){
               UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
        }
    }
}
