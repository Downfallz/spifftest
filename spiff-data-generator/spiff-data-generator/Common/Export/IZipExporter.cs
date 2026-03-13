namespace spiff_data_generator.Common.Export;

public interface IZipExporter
{
    Action<int, int>? OnProgress { get; set; }
    void ExportToFile();
    void ExportToStream(Stream output, string? yyyymmdd = null);
}
