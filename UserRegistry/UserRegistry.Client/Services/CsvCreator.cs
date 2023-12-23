using Microsoft.JSInterop;
using System.Text;
using System.Text.Json;
using UserRegistry.Client.Models;

namespace UserRegistry.Client.Services;

public class CsvCreator(IJSRuntime jSRuntime, List<PersonModel> people)
{
    private IJSRuntime JSRuntime { get; set; } = jSRuntime;
    private readonly List<PersonModel> people = people;
    private const char csvSeparator = ',';

    private const string dateTimeFormatStringCsvExport =
        "yyyy'-'MM'-'dd'-'HH:mm:ss";

    private static readonly string[] visibleColumns =
        ["Number", "Id", "Name", "Address", "Phone"];

    private const string fileNameBaseCsvExport = "scrolled-data-";
    private const string fileExtension = "csv";

    public async Task SaveToCsv()
    {
        var t = DateTime.Now;
        string fileName = $"{fileNameBaseCsvExport}" +
            $"{t.ToString(dateTimeFormatStringCsvExport)}";
        await DownloadObject(people, $"{fileName}.{fileExtension}");
    }

    private async Task DownloadObject(object objectToSave, string fileName)
    {
        GetDataForCsvString(objectToSave, out List<string[]> totalRows);
        string csv = CreateCsvString(totalRows);
        using DotNetStreamReference streamRef = CreateFileStreamForCsvFile(csv);
        await JSRuntime.InvokeVoidAsync("downloadFileFromStream",
            fileName, streamRef);
    }

    private static DotNetStreamReference CreateFileStreamForCsvFile(string csv)
    {
        var fileStream = new MemoryStream(new UTF8Encoding(true).GetBytes(csv));
        var streamRef = new DotNetStreamReference(stream: fileStream);
        return streamRef;
    }

    private static void GetDataForCsvString(object objectToSave,
        out List<string[]> totalRows)
    {
        SerializeData(objectToSave, out JsonDocument document,
            out JsonElement.ArrayEnumerator root);
        totalRows = [];
        GetSerializedData(totalRows, root);
    }

    private static void GetSerializedData(List<string[]> totalRows,
        JsonElement.ArrayEnumerator root)
    {
        if (root.Any())
        {
            GetDataRows(root, totalRows);
        }
    }

    private static string CreateCsvString(List<string[]> totalRows)
    {
        string[] columnNames = visibleColumns;
        var rows = totalRows.ToArray();
        var csv = Csv.CsvWriter.WriteToText(columnNames, rows, csvSeparator);
        return csv;
    }

    private static void SerializeData(object objectToSave,
        out JsonDocument document, out JsonElement.ArrayEnumerator root)
    {
        document = JsonSerializer.SerializeToDocument(objectToSave);
        root = document.RootElement.EnumerateArray();
    }

    private static void GetDataRows(JsonElement.ArrayEnumerator root,
        List<string[]> totalRows)
    {
        foreach (var element in root)
        {
            AddDataRow(totalRows, element);
        }
    }

    private static void AddDataRow(List<string[]> totalRows, JsonElement element)
    {
        var row = element.EnumerateObject().Select(o => o.Value.ToString());
        List<string> rowsResult = [];
        rowsResult.AddRange(row);
        totalRows.Add([.. rowsResult]);
    }
}