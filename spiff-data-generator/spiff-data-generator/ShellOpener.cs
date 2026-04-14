using Spectre.Console;

namespace spiff_data_generator;
public static class ShellOpener
{
    public static void Open(string? path)
    {
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            var fullPath = Path.GetFullPath(path);
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(fullPath){
               UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
        }
    }
}
