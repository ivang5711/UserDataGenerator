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
            u.Number = f.IndexGlobal + 1;
            u.Id = f.Database.Random.Uuid().ToString();
            u.Gender = f.Person.Gender.ToString();
            u.FirstName = f.Name.FirstName();
            u.LastName = f.Name.LastName();
            u.MiddleName = f.Name.FirstName(f.Person.Gender).OrNull(f, .2f);
            u.Email = f.Internet.Email(u.FirstName, u.LastName);
            u.Phone = f.Phone.PhoneNumber();
            u.StreetAddress = f.Person.Address.Street;
            u.City = f.Person.Address.City;
            u.State = f.Person.Address.State;
            u.ZipCode = f.Person.Address.ZipCode;
            u.Suite = f.Person.Address.Suite;
            u.Name = CombineName(u.FirstName, u.MiddleName, u.LastName);
            u.Address = ShuffleAddress(u.Name, u.City, u.Email, u.Phone,
            u.StreetAddress, u.Suite, u.ZipCode, u.State);
        });

    private readonly Faker<LocalizedLetters> lettersFaker =
        new Faker<LocalizedLetters>(locale)
        .StrictMode(false)
        .Rules((f, u) =>
        {
            u.Letters = f.Person.FirstName;
            u.Digits = f.Finance.Account();
            u.AlfaNumericSet = CombineAlfanumeric(u.Letters, u.Digits);
        }
            );

    public LocalizedLetters GenerateLetters(int seed)
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
        string name, string city, string email, string phone,
        string streetAddress, string suite, string zipCode, string state)
    {
        StringBuilder sb = new();
        if (email.Length > name.Length)
        {
            sb.Append(suite).Append(streetAddress).Append(city)
                .Append(zipCode).Append(state).Append(' ');
            return sb.ToString();
        }

        if (phone.Length > email.Length)
        {
            sb.Append(zipCode).Append(city).Append(state)
                .Append(suite).Append(streetAddress).Append(' ');
            return sb.ToString();
        }

        sb.Append(state).Append(city).Append(streetAddress)
            .Append(suite).Append(zipCode).Append(' ');
        return sb.ToString();
    }

    private static string CombineName(string firstName,
        string MiddleName, string LastName)
    {
        StringBuilder sb = new();
        sb.Append(firstName).Append(MiddleName).Append(LastName);
        return sb.ToString();
    }
}