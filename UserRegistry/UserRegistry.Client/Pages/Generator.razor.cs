using Microsoft.JSInterop;
using System.Text;
using UserRegistry.Client.Models;

namespace UserRegistry.Client.Pages;

public partial class Generator
{
    private const int startCounter = 1;
    private const int usersDataChunkSize = 5;
    private const int usersDataChunkStartSize = 20;
    private const string errorValueStep = "0.25";
    private const string errorMinValue = "0";
    private const string seedMaxRandomValue = "1999999999";
    private const string userSeedDefaultValue = "0";
    private const string localeDefaultValue = "en";
    private string locale = localeDefaultValue;
    private decimal errorValue = 0;
    private int localeValue = 0;
    private int seed = 0;
    private int counter = 1;
    private string _userSeed = userSeedDefaultValue;
    private DataGenerator data = new(localeDefaultValue);
    private List<PersonModel> people = [];
    private bool doubleRender = false;

    private static readonly Dictionary<string, string> locales = new()
    {
        {"USA", "en" },
        { "Germany", "de" },
        { "Canada", "en_CA" },
        { "France", "fr" }
    };

    protected override async Task OnInitializedAsync()
    {
        await Task.Yield();
        InitializeGenerator();
    }

    private string? UserSeed
    {
        get => _userSeed;
        set => _userSeed =
            value is not null && int.TryParse(value.Trim(), out _) ?
            value.Trim() : userSeedDefaultValue;
    }

    private void GenerateChunkOfUsers()
    {
        if (doubleRender)
        {
            doubleRender = false;
            if (people.Count < usersDataChunkStartSize)
            {
                ResetPeopleStorage();
                AddUsersToPeople(usersDataChunkStartSize);
            }
            else
            {
                AddUsersToPeople(usersDataChunkSize);
            }
        }
        else
        {
            doubleRender = true;
            AddUsersToPeople(usersDataChunkSize);
        }
    }

    private void AddUsersToPeople(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            AddUserToPeople();
        }
    }

    private void AddUserToPeople()
    {
        PersonModel user = data.GeneratePerson(seed++);
        user.Number = counter++;
        people.Add(user);
    }

    private void InitializeGenerator()
    {
        ResetPeopleStorage();
        SetLocaleValue();
        SetSeed();
        data = new(locale);
    }

    private void SetSeed()
    {
        seed = int.Parse(UserSeed is not null ?
            UserSeed : userSeedDefaultValue);
        seed += (int)Math.Floor(errorValue * 100) + localeValue;
    }

    private void ResetPeopleStorage()
    {
        people.Clear();
        counter = startCounter;
    }

    private void GenerateRandomSeed()
    {
        Random t = new();
        UserSeed = t.Next(int.Parse(seedMaxRandomValue)).ToString();
        InitializeGenerator();
    }

    private void SetLocaleValue()
    {
        localeValue = 0;
        foreach (char item in locale)
        {
            localeValue += Convert.ToInt32(item);
        }
    }

    private async Task SaveToCsv()
    {
        await downloadObject(people, "people.csv");
    }

    private async Task downloadObject(object objectToSave, string fileName)
    {
        using var document = System.Text.Json.JsonSerializer.SerializeToDocument(objectToSave);
        var root = document.RootElement.EnumerateArray();
        List<string> headersResult = [];
        List<string[]> totalRows = [];

        if (root.Any())
        {
            var headers = root.First().EnumerateObject().Select(o => o.Name);
            headersResult.AddRange(headers);
            foreach (var element in root)
            {
                var row = element.EnumerateObject().Select(o => o.Value.ToString());
                List<string> rowsResult = [];
                rowsResult.AddRange(row);
                totalRows.Add(rowsResult.ToArray());
            }
        }

        var columnNames = headersResult.ToArray();
        var rows = totalRows.ToArray();
        var csv = Csv.CsvWriter.WriteToText(columnNames, rows, ',');
        var fileStream = new MemoryStream(new UTF8Encoding(true).GetBytes(csv));
        using var streamRef = new DotNetStreamReference(stream: fileStream);
        await JSRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }
}