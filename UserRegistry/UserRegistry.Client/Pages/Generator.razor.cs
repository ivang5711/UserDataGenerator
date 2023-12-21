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
    { "Germany", "de" },
    { "Canada", "en_CA" },
    { "France", "fr" }
};

    protected override async Task OnInitializedAsync()
    {
        await IitializeGenerator();
    }

    private string? UserSeed
    {
        get => _userSeed;
        set => _userSeed =
            value is not null && int.TryParse(value.Trim(), out _) ?
            value.Trim() : userSeedDefaultValue;
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
        AddUsersToPeople(amount);
        return people;
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

    private async Task<IEnumerable<PersonModel>> GetUsersData()
    {
        var task = Task.Run(() => ReturnUsers(usersDataChunkSize));
        var myOutput = await task;
        return myOutput;
    }

    private async Task IitializeGenerator()
    {
        ReserPeopleStorage();
        SetLocaleValue();
        SetSeed();
        data = new(locale);
        await Task.Run(() => ReturnUsers(usersDataChunkSize));
    }

    private void SetSeed()
    {
        seed = int.Parse(UserSeed is not null ? UserSeed : "0");
        seed += (int)Math.Floor(errorValue * 100) + localeValue;
    }

    private void ReserPeopleStorage()
    {
        people.Clear();
        counter = startCounter;
    }

    private async Task GenerateRandomSeed()
    {
        Random t = new();
        UserSeed = t.Next(int.Parse(seedMaxRandomValue)).ToString();
        await IitializeGenerator();
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