using Bogus;
using Domain.Models;

namespace Server.Data;

public class ProductGenerator : IGenerator<Product>
{
    private readonly Faker<Product> _faker;

    public ProductGenerator()
    {
        _faker = new Faker<Product>()
            .RuleFor(e => e.Id, f => Guid.NewGuid())
            .RuleFor(e => e.CreatedAt, f => f.Date.Recent(0))
            .RuleFor(e => e.EAN, f => f.Commerce.Ean13())
            .RuleFor(e => e.ManufacturedAt, f => f.Random.Int()) //ManufacturedAt is int by condition
            .RuleFor(e => e.Price, f => f.Random.Double()) // Price is double by condition, but in commerce price is string
            .RuleFor(e => e.Description, f => f.Commerce.ProductDescription());
    }

    public Product Generate() => _faker.Generate();

    public IEnumerable<Product> GenerateSet(int number) => _faker.Generate(number);
}