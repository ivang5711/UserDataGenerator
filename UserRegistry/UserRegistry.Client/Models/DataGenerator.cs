using Bogus;

namespace UserRegistry.Client.Models;

public class DataGenerator(string locale)
{
    private readonly Faker<PersonModel> personModelFake = new Faker<PersonModel>(locale)
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
            u.Rating = f.PickRandom<CreditRating>();
            u.Name = $"{u.FirstName} {u.MiddleName} {u.LastName}";
            u.Address = ShuffleAddress(u.Name, u.City, u.Email, u.Phone,
                u.StreetAddress, u.Suite, u.ZipCode, u.State);
        });

    public PersonModel GeneratePerson(int seed)
    {
        return personModelFake.UseSeed(seed).Generate();
    }

    private static string ShuffleAddress(
        string name, string city, string email, string phone,
        string streetAddress, string suite, string zipCode, string state)
    {
        if (email.Length > name.Length)
        {
            return $"{suite} {streetAddress} {city} {zipCode} {state}";
        }

        if (phone.Length > email.Length)
        {
            return $"{zipCode} {city} {state} {suite} {streetAddress} ";
        }

        return $"{state} {city} {streetAddress} {suite} {zipCode}";
    }
}