using Bogus;

namespace UserRegistry.Client.Models;

public class DataGenerator(string locale)
{
    private readonly Faker<PersonModel> personModelFake = new Faker<PersonModel>(locale)
        .StrictMode(false)
        .Rules((f, u) =>
        {
            u.Number = f.IndexGlobal + 1;
            u.Id = f.Random.Int(1, 10000);
            u.Gender = f.Person.Gender.ToString();
            u.FirstName = f.Name.FirstName();
            u.LastName = f.Name.LastName();
            u.MiddleName = f.Name.FirstName(f.Person.Gender).OrNull(f, .2f);
            u.Email = f.Internet.Email(u.FirstName, u.LastName);
            u.Phone = f.Phone.PhoneNumber();
            u.StreetAddress = f.Address.FullAddress();
            u.City = f.Address.City();
            u.State = f.Address.StateAbbr();
            u.ZipCode = f.Address.ZipCode();
            u.Rating = f.PickRandom<CreditRating>();
            u.Name = $"{u.FirstName} {u.MiddleName} {u.LastName}";
        });

    public PersonModel GeneratePerson(int seed)
    {
        return personModelFake.UseSeed(seed).Generate();
    }

    public IEnumerable<PersonModel> GeneratePeople(int seed)
    {
        return personModelFake.UseSeed(seed).GenerateForever();
    }

    public IEnumerable<PersonModel> GeneratePeople()
    {
        return personModelFake.GenerateForever();
    }
}