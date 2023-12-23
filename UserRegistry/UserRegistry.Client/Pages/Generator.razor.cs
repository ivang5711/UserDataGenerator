using MathNet.Numerics.Distributions;
using UserRegistry.Client.Models;
using UserRegistry.Client.Services;

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
    private RandomErrorCreator? errorCreator;
    private decimal errorProbability = 0;

    public async Task Download()
    {
        CsvCreator csvCreator = new(JSRuntime, people);
        await csvCreator.SaveToCsv();
    }

    private static readonly Dictionary<string, string> locales = new()
    {
        { "USA", "en" },
        { "Germany", "de" },
        { "Canada", "en_CA" },
        { "France", "fr" },
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

    private void GetErrorProbability()
    {
        decimal temp = Math.Floor(errorValue);
        decimal result = errorValue - temp;
        errorProbability = result > 0 ? result : 0;
    }

    private void GetErrorWithProbability()
    {
        if (errorProbability != 0)
        {
            CalculateErrorWithProbability();
        }
    }

    private void CalculateErrorWithProbability()
    {
        RollErrorDice(out int multiplier, out double dice);
        if (dice <= (double)Math.Round(errorProbability * multiplier))
        {
            errorValue++;
        }
    }

    private void RollErrorDice(out int multiplier, out double dice)
    {
        multiplier = 10000;
        var t = new Random(seed + (int)errorValue);
        var continuousDistribution = new ContinuousUniform(0, multiplier, t);
        dice = continuousDistribution.Sample();
    }

    private void GenerateChunkOfUsers()
    {
        if (doubleRender)
        {
            doubleRender = false;
            CheckFirstRender();
        }
        else
        {
            doubleRender = true;
            AddUsersToPeople(usersDataChunkSize);
        }
    }

    private void CheckFirstRender()
    {
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

    private void AddUsersToPeople(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            AddUserToPeople();
        }
    }

    private void InitializeGenerator()
    {
        RefreshState();
        data = new(locale);
        errorCreator = new(seed, errorValue, data);
        errorCreator.ResetErrorCounters();
    }

    private void RefreshState()
    {
        ResetPeopleStorage();
        SetLocaleValue();
        SetUpErrorValue();
        SetSeed();
    }

    private void SetUpErrorValue()
    {
        GetErrorProbability();
        GetErrorWithProbability();
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

    private void AddUserToPeople()
    {
        errorCreator!.DefineErrorDistribution();
        PersonModel user = data.GeneratePerson(seed++);
        UpdateUserData(user);
        people.Add(user);
        errorCreator.ResetErrorCounters();
    }

    private void UpdateUserData(PersonModel user)
    {
        user.Number = counter++;
        user.Name = errorCreator!.AddMistakes(
            user.Name, errorCreator.NameMistakesCount);
        user.Address = errorCreator.AddMistakes(
            user.Address, errorCreator.AddressMistakesCount);
        user.Phone = errorCreator.AddMistakes(
            user.Phone, errorCreator.PhoneMistakesCount);
    }
}