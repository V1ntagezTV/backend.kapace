using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Models;
using backend.kapace.DAL.Models;
using backend.Models.Enums;
using Bogus;

namespace backend.Tests.Helpers;

internal class RandomDataModelFactory 
{
    public static InsertContentQuery CreateInsertContentQuery() 
    {
        return new Faker<InsertContentQuery>()
            .RuleFor(m => m.Id, f => f.Random.Int(0))
            .RuleFor(m => m.ImageId, f => f.Random.Int())
            .RuleFor(m => m.Title, f => f.Random.String2(10))
            .RuleFor(m => m.Description, f => f.Random.String2(10))
            .RuleFor(m => m.ContentType, f => (int) f.PickRandom<ContentType>())
            .RuleFor(m => m.Country, f => (int) f.PickRandom<Country>())
            .RuleFor(m => m.OriginTitle, f => f.Random.String2(10))
            .RuleFor(m => m.EngTitle, f => f.Random.String2(10))
            .RuleFor(m => m.Status, f => (int) f.PickRandom<ContentStatus>())
            .RuleFor(m => m.Channel, f => f.Random.String2(10))
            .RuleFor(m => m.MinAge, f => f.Random.Int(1, 18))
            .RuleFor(m => m.Duration, f => f.Random.Int(0))
            .RuleFor(m => m.PlannedSeries, f => f.Random.Int(0))
            .RuleFor(m => m.ImportStars, f => f.Random.Int(0))
            .RuleFor(m => m.OutSeries, f => f.Random.Int(0))
            .RuleFor(m => m.Views, f => f.Random.Int(0))
            .RuleFor(m => m.ReleasedAt, f => DateTimeOffset.UtcNow)
            .RuleFor(m => m.CreatedAt, f => DateTimeOffset.UtcNow)
            .RuleFor(m => m.LastUpdateAt, f => DateTimeOffset.UtcNow)
            .Generate();
    }

    public static InsertContentModel CreateBllContentQuery() 
    {
        return new Faker<InsertContentModel>()
            .CustomInstantiator(f => new InsertContentModel(
                Id: f.Random.Int(0),
                ImageId: f.Random.Int(),
                Title: f.Random.String2(10),
                Description: f.Random.String2(10),
                ContentType: f.PickRandom<ContentType>(),
                Country: f.PickRandom<Country>(),
                OriginTitle: f.Random.String2(10),
                EngTitle: f.Random.String2(10),
                Status: f.PickRandom<ContentStatus>(),
                Channel: f.Random.String2(10),
                MinAge: f.Random.Int(1, 18),
                Duration: f.Random.Int(1, 280),
                PlannedSeries: f.Random.Int(1, 3000),
                ReleasedAt: DateTimeOffset.UtcNow)
                //.RuleFor(m => m.ImportStars, f => f.Random.Int(0))
                //.RuleFor(m => m.OutSeries, f => f.Random.Int(0))
                //.RuleFor(m => m.Views, f => f.Random.Int(0))
                //.RuleFor(m => m.CreatedAt, f => DateTimeOffset.UtcNow)
                //.RuleFor(m => m.LastUpdateAt, f => DateTimeOffset.UtcNow)
            ).Generate();
    }
}