namespace spiff_data_generator;

public static class DebugController
{
    public static bool Enabled { get; set; } = false;

    public static void Log(string message)
    {
        if (Enabled)
            Console.WriteLine($"[DEBUG] {message}");
    }
}
