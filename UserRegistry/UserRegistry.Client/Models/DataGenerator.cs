using Bogus;

namespace UserRegistry.Client.Models;

public class DataGenerator(string locale)
{
    private readonly Faker<PersonModel> personModelFake = new Faker<PersonModel>(locale)
            .RuleFor(u => u.Number, f => f.IndexGlobal + 1)
            .RuleFor(u => u.Id, f => f.Random.Int(1, 10000))
            .RuleFor(u => u.Gender, f => f.Person.Gender.ToString())
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.MiddleName, f =>
                f.Name.FirstName(f.Person.Gender).OrNull(f, .2f))
            .RuleFor(u => u.Email, (f, u) =>
                f.Internet.Email(u.FirstName, u.LastName))
            .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(u => u.StreetAddress, f => f.Address.FullAddress())
            .RuleFor(u => u.City, f => f.Address.City())
            .RuleFor(u => u.State, f => f.Address.StateAbbr())
            .RuleFor(u => u.ZipCode, f => f.Address.ZipCode())
            .RuleFor(u => u.Rating, f => f.PickRandom<CreditRating>())
            .RuleFor(u => u.Name, f =>
            $"{f.Name.FirstName()} " +
            $"{f.Name.FirstName(f.Person.Gender).OrNull(f, .2f)} " +
            $"{f.Name.LastName()}");

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