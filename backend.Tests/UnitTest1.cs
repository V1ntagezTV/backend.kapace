using backend.kapace.DAL.Experimental;
using backend.kapace.DAL.Experimental.StarsRepository;
using backend.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Tests;

public class UnitTest1 : IClassFixture<MainFixture>
{
    private readonly MainFixture _mainFixture;

    public UnitTest1(MainFixture mainFixture)
    {
        _mainFixture = mainFixture;
    }

    [Fact]
    public async Task Select_Validate_ShouldPass()
    {
        var a = _mainFixture.ServiceProvider;
        var experimentalRepository = a.GetService<BaseRepository<StarsDataColumns>>() ?? throw new NullReferenceException("PIZDA");
    }
}