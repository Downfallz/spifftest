namespace spiff_data_generator;

public interface IZipExporter
{
    void ExportToFile();
    void ExportToStream(Stream output, string? yyyymmdd = null);
}
