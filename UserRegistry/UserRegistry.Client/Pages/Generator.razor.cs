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
}