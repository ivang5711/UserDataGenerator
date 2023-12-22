using MathNet.Numerics.Distributions;
using Microsoft.JSInterop;
using System.Text;
using System.Text.Json;
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
    private const char csvSeparator = ',';

    private const string dateTimeFormatStringCsvExport =
        "yyyy'-'MM'-'dd'-'HH:mm:ss";

    private const string fileNameBaseCsvExport = "scrolled-data-";
    private const string fileExtension = "csv";
    private string locale = localeDefaultValue;
    private decimal errorValue = 0;
    private int localeValue = 0;
    private int seed = 0;
    private int counter = 1;
    private string _userSeed = userSeedDefaultValue;
    private DataGenerator data = new(localeDefaultValue);
    private List<PersonModel> people = [];
    private bool doubleRender = false;
    private int _nameMistakesCount;
    private int _addressMistakesCount;
    private int _phoneMistakesCount;
    private int _errorPosition;
    private int _errorType;
    private int errorCharCounter = 0;

    private static readonly Dictionary<string, string> locales = new()
    {
        {"USA", "en" },
        { "Germany", "de" },
        { "Canada", "en_CA" },
        { "France", "fr" },
        { "Russia", "ru" },
        { "Korea", "ko" }
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

    private void InitializeGenerator()
    {
        ResetErrorCounters();
        ResetPeopleStorage();
        SetLocaleValue();
        SetSeed();
        data = new(locale);
    }

    private void ResetErrorCounters()
    {
        _nameMistakesCount = 0;
        _addressMistakesCount = 0;
        _phoneMistakesCount = 0;
        _errorPosition = 0;
        _errorType = 0;
    }

    private void SetSeed()
    {
        seed = int.Parse(UserSeed is not null ?
            UserSeed : userSeedDefaultValue);
        seed += localeValue;
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
        var t = DateTime.Now;
        string fileName = $"{fileNameBaseCsvExport}" +
            $"{t.ToString(dateTimeFormatStringCsvExport)}";
        await DownloadObject(people, $"{fileName}.{fileExtension}");
    }

    private async Task DownloadObject(object objectToSave, string fileName)
    {
        GetDataForCsvString(objectToSave, out List<string> headersResult,
            out List<string[]> totalRows);
        string csv = CreateCsvString(headersResult, totalRows);
        var fileStream = new MemoryStream(new UTF8Encoding(true).GetBytes(csv));
        using var streamRef = new DotNetStreamReference(stream: fileStream);
        await JSRuntime.InvokeVoidAsync("downloadFileFromStream",
            fileName, streamRef);
    }

    private static void GetDataForCsvString(object objectToSave,
        out List<string> headersResult, out List<string[]> totalRows)
    {
        SerializeData(objectToSave, out JsonDocument document,
            out JsonElement.ArrayEnumerator root);
        headersResult = [];
        totalRows = [];
        GetSerializedData(headersResult, totalRows, root);
    }

    private static void GetSerializedData(List<string> headersResult,
        List<string[]> totalRows, JsonElement.ArrayEnumerator root)
    {
        if (root.Any())
        {
            GetDataRows(root, headersResult, totalRows);
        }
    }

    private static string CreateCsvString(List<string> headersResult,
        List<string[]> totalRows)
    {
        var columnNames = headersResult.ToArray();
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
        List<string> headersResult, List<string[]> totalRows)
    {
        AddDataHeader(root, headersResult);
        foreach (var element in root)
        {
            AddDataRow(totalRows, element);
        }
    }

    private static void AddDataHeader(JsonElement.ArrayEnumerator root,
        List<string> headersResult)
    {
        var headers = root.First().EnumerateObject().Select(o => o.Name);
        headersResult.AddRange(headers);
    }

    private static void AddDataRow(List<string[]> totalRows, JsonElement element)
    {
        var row = element.EnumerateObject().Select(o => o.Value.ToString());
        List<string> rowsResult = [];
        rowsResult.AddRange(row);
        totalRows.Add([.. rowsResult]);
    }

    private void AddUserToPeople()
    {
        DefineErrorDistribution();
        PersonModel user = data.GeneratePerson(seed++);
        UpdateUserData(user);
        people.Add(user);
        ResetErrorCounters();
    }

    private void UpdateUserData(PersonModel user)
    {
        user.Number = counter++;
        user.Name = AddMistakes(user.Name, _nameMistakesCount);
        user.Address = AddMistakes(user.Address, _addressMistakesCount);
        user.Phone = AddMistakes(user.Phone, _phoneMistakesCount);
    }

    private static string DeleteSymbolError(string input, int position)
    {
        char[] chars = input.ToCharArray();
        char[] res = new char[chars.Length - 1];
        int a = 0;
        for (int i = 0; i < chars.Length; i++)
        {
            if (i == position)
            {
                continue;
            }
            res[a] = chars[i];
            a++;
        }

        return new string(res);
    }

    private string AddSymbolError(string input, int position)
    {
        char[] chars = input.ToCharArray();
        char[] res = new char[chars.Length + 1];
        int a = 0;
        for (int i = 0; i < chars.Length; i++)
        {
            if (i == position)
            {
                res[a] = GetChar();
                a++;
            }
            res[a] = chars[i];
            a++;
        }

        return new string(res);
    }

    private static string ReplaceNeighboursError(string input, int position)
    {
        char[] chars = input.ToCharArray();
        (chars[position - 1], chars[position]) =
            (chars[position], chars[position - 1]);
        return new string(chars);
    }

    private int DefineErrorType()
    {
        var t = new Random(seed + (int)errorValue + _errorType++);
        return GetNormalDistributedValue(0, 4, t);
    }

    private string AddRandomError(string input)
    {
        if (input.Length > 3)
        {
            int type = DefineErrorType();
            int position = DefineErrorPosition(input);
            if (type == 1)
            {
                return AddSymbolError(input, position);
            }
            else if (type == 2)
            {
                return ReplaceNeighboursError(input, position);
            }
            else
            {
                return DeleteSymbolError(input, position);
            }
        }

        return input;
    }

    private int DefineErrorPosition(string name)
    {
        var t = new Random(seed + (int)errorValue + _errorPosition++);
        return GetNormalDistributedValue(0, name.Length - 1, t);
    }

    private string AddMistakes(string name, int count)
    {
        string tmp = name;
        if (name.Length > 3)
        {
            for (int i = 0; i < count; i++)
            {
                tmp = AddRandomError(tmp);
            }
        }

        return tmp;
    }

    private void DefineErrorDistribution()
    {
        var t = new Random(seed + (int)errorValue);
        for (int i = 0; i < (int)errorValue; i++)
        {
            int temp = GetNormalDistributedValue(0, 4, t);

            if (temp == 1)
            {
                _nameMistakesCount++;
            }
            else if (temp == 2)
            {
                _addressMistakesCount++;
            }
            else
            {
                _phoneMistakesCount++;
            }
        }
    }

    private static int GetNormalDistributedValue(
        int lower, int upper, Random random)
    {
        double sample;
        var continuousDistribution =
            new ContinuousUniform(lower, upper, random);
        do
        {
            sample = RetrieveSample(continuousDistribution);
        } while (sample < lower + 1 || sample > upper - 1);
        return Convert.ToInt32(sample);
    }

    private static double RetrieveSample(
        ContinuousUniform continuousDistribution)
    {
        double sample;
        double continuusDistribution = continuousDistribution.Sample();
        sample = Math.Round(continuusDistribution);
        return sample;
    }

    private char GetChar()
    {
        string result = data.GenerateLetters(seed).AlfaNumericSet;
        var t = new Random(seed + (int)errorValue +
            errorCharCounter++ + result.Length);
        int index = GetNormalDistributedValue(0, result.Length - 1, t);
        return result[index];
    }
}