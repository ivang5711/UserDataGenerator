using Bogus;
using System.Text;

namespace UserRegistry.Client.Models;

public class DataGenerator(string locale)
{
    private readonly Faker<PersonModel> personModelFake =
        new Faker<PersonModel>(locale)
        .StrictMode(false)
        .Rules((f, u) =>
        {
            u.Id = f.Database.Random.Uuid().ToString();
            u.Phone = f.Phone.PhoneNumber();
            u.Name = CombineName(f.Name.FirstName(),
                f.Name.FirstName(f.Person.Gender).OrNull(f, .2f),
                f.Name.LastName());
            u.Address = ShuffleAddress(u.Name,
                f.Person.Address.City, u.Phone,
            f.Person.Address.Street, f.Person.Address.Suite,
            f.Person.Address.ZipCode, f.Person.Address.State);
        });

    private readonly Faker<LocalizedAlfanumeric> lettersFaker =
        new Faker<LocalizedAlfanumeric>(locale)
        .StrictMode(false)
        .Rules((f, u) =>
        {
            u.AlfaNumericSet = CombineAlfanumeric(f.Person.FullName,
                f.Finance.Account());
        });

    public LocalizedAlfanumeric GenerateLetters(int seed)
    {
        return lettersFaker.UseSeed(seed).Generate();
    }

    public PersonModel GeneratePerson(int seed)
    {
        return personModelFake.UseSeed(seed).Generate();
    }

    private static string CombineAlfanumeric(string letters, string digits)
    {
        StringBuilder sb = new();
        sb.Append(letters).Append(digits);
        return sb.ToString();
    }

    private static string ShuffleAddress(
        string name, string city, string phone,
        string streetAddress, string suite, string zipCode, string state)
    {
        StringBuilder sb = new();
        if (suite.Length + zipCode.Length > name.Length)
        {
            CreateFirstAddress(city, streetAddress, suite, zipCode, state, sb);
            return sb.ToString();
        }

        if (phone.Length > city.Length + suite.Length)
        {
            CreateSecondAddress(city, streetAddress, suite, zipCode, state, sb);
            return sb.ToString();
        }

        CreateDefaultAddress(city, streetAddress, suite, zipCode, state, sb);
        return sb.ToString();
    }

    private static void CreateDefaultAddress(string city, string streetAddress,
        string suite, string zipCode, string state, StringBuilder sb)
    {
        sb.Append(state).Append(' ').Append(city).Append(' ')
            .Append(streetAddress).Append(' ').Append(suite).Append(' ')
            .Append(zipCode).Append(' ');
    }

    private static void CreateSecondAddress(string city, string streetAddress,
        string suite, string zipCode, string state, StringBuilder sb)
    {
        sb.Append(zipCode).Append(' ').Append(city).Append(' ')
            .Append(state).Append(' ').Append(suite).Append(' ')
            .Append(streetAddress).Append(' ');
    }

    private static void CreateFirstAddress(string city, string streetAddress,
        string suite, string zipCode, string state, StringBuilder sb)
    {
        sb.Append(suite).Append(' ').Append(streetAddress)
            .Append(' ').Append(city).Append(' ').Append(zipCode)
            .Append(' ').Append(state).Append(' ');
    }

    private static string CombineName(string firstName,
        string MiddleName, string LastName)
    {
        StringBuilder sb = new();
        sb.Append(firstName).Append(' ').Append(MiddleName)
            .Append(' ').Append(LastName);
        return sb.ToString();
    }
}