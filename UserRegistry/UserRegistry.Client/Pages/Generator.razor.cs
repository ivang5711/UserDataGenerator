using Microsoft.AspNetCore.Components.Web.Virtualization;
using UserRegistry.Client.Models;

namespace UserRegistry.Client.Pages;

public partial class Generator
{
    private const int startCounter = 1;
    private const int usersDataChunkSize = 20;
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

    private static readonly Dictionary<string, string> locales = new()
{
    {"USA", "en" },
    { "Georgia", "ge" },
    { "Germany", "de" },
    { "Korea", "ko" },
    { "China", "zh_CN" },
    { "Czech", "cz" },
    { "Slovakia", "sk" },
    { "Russia", "ru" },
    { "Greece", "el" },
    { "Japan", "ja" },
};

    protected override async Task OnInitializedAsync()
    {
        await ClearPeopleStorage();
    }

    private string? UserSeed
    {
        get => _userSeed;
        set
        {
            _userSeed = value is not null && int.TryParse(value.Trim(), out _) ?
            value.Trim() : userSeedDefaultValue;
        }
    }

    private async ValueTask<ItemsProviderResult<PersonModel>> LoadUserData(
        ItemsProviderRequest request)
    {
        await GetUsersData();
        IEnumerable<PersonModel> t = people.Skip(request.StartIndex)
        .Take(people.Count);
        return new ItemsProviderResult<PersonModel>(
            t, people.Count + usersDataChunkSize);
    }

    private List<PersonModel> ReturnUsers(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            PersonModel user = data.GeneratePerson(seed++);
            user.Number = counter++;
            people.Add(user);
        }

        return people;
    }

    private async Task<IEnumerable<PersonModel>> GetUsersData()
    {
        var task = Task.Run(() => ReturnUsers(usersDataChunkSize));
        var myOutput = await task;
        return myOutput;
    }

    private async Task ClearPeopleStorage()
    {
        people.Clear();
        seed = int.Parse(UserSeed is not null ? UserSeed : "0");
        SetLocaleValue();
        seed += (int)Math.Floor(errorValue * 100) + localeValue;
        data = new(locale);
        counter = startCounter;
        await Task.Run(() => ReturnUsers(usersDataChunkSize));
        StateHasChanged();
    }

    private async Task GenerateRandomSeed()
    {
        Random t = new();
        UserSeed = t.Next(int.Parse(seedMaxRandomValue)).ToString();
        await ClearPeopleStorage();
    }

    private void SetLocaleValue()
    {
        localeValue = 0;
        char[] f = locale.ToCharArray();
        foreach (var item in f)
        {
            localeValue += Convert.ToInt32(item);
        }
    }
}