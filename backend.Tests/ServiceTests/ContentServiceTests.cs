using backend.kapace.BLL.Models.VideoService;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Models;
using backend.Tests.Fixtures;
using backend.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Tests.ServiceTests;


public class ContentServiceTests : IClassFixture<MainFixture>
{
    private readonly MainFixture _mainFixture;
    private IServiceProvider _provider;
    private IContentService _contentProvider;

    public ContentServiceTests(MainFixture mainFixture)
    {
        _mainFixture = mainFixture;
        _provider = mainFixture.ServiceProvider;
        _contentProvider = mainFixture.ServiceProvider
            .GetService<IContentService>()
            ?? throw new NullReferenceException(nameof(IContentRepository));
    }

    [Fact]
    public async Task QueryAsync__HappyPath__ShouldPass() 
    {
        // Arrange
        var rndContent = RandomDataModelFactory.CreateBllContentQuery();

        // Act
        var contentId = await _contentProvider.InsertAsync(rndContent, CancellationToken.None);

        // Assert
        var contents = await _contentProvider.QueryAsync(
            new kapace.BLL.Models.ContentQuery() {Ids = new[] {contentId}, Limit = 20}, 
            CancellationToken.None
        );

        contents.Should().NotBeEmpty();
        var unit = contents.Single();
        unit.Id.Should().Be(contentId);
        unit.Title.Should().Be(rndContent.Title);
        unit.EngTitle.Should().Be(rndContent.EngTitle);
        unit.OriginTitle.Should().Be(rndContent.OriginTitle);
        unit.Description.Should().Be(rndContent.Description);
        unit.ContentType.Should().Be(rndContent.ContentType);
        unit.Status.Should().Be(rndContent.Status);
        unit.ImageId.Should().Be(rndContent.ImageId);
        unit.ImportStars.Should().Be(0); //rndContent.ImportStars);
        unit.OutSeries.Should().Be(0);
        unit.PlannedSeries.Should().Be(rndContent.PlannedSeries);
        unit.Views.Should().Be(0); //rndContent.Views); 
        unit.Country.Should().Be(rndContent.Country);
        unit.ReleasedAt.Should().BeCloseTo(rndContent.ReleasedAt.Value, TimeSpan.FromMinutes(1));
        unit.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        unit.LastUpdateAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        unit.MinAge.Should().Be(rndContent.MinAge);
        unit.Duration.Should().Be(rndContent.Duration);
    }  
    
    [Theory]
    [InlineData(null, kapace.BLL.Enums.ContentSelectedInfo.None)]
    [InlineData(null, kapace.BLL.Enums.ContentSelectedInfo.ContentGenres)]
    [InlineData(null, kapace.BLL.Enums.ContentSelectedInfo.Translations)]
    [InlineData(null, kapace.BLL.Enums.ContentSelectedInfo.UserInfo)]
    [InlineData(null, kapace.BLL.Enums.ContentSelectedInfo.Episodes)]
    [InlineData("", kapace.BLL.Enums.ContentSelectedInfo.None)]
    public async Task GetByQueryAsync__HappyPath__ShouldPass(
        string search, 
        kapace.BLL.Enums.ContentSelectedInfo selectedInfo)
    {
        // Arrange
        var rndContent = RandomDataModelFactory.CreateBllContentQuery();

        // Act
        var contentId = await _contentProvider.InsertAsync(rndContent, CancellationToken.None);
        var contents = await _contentProvider.GetByQueryAsync(
            search: search,
            searchFilters: new kapace.BLL.Models.SearchFilters() { 
                ContentIds = new[] {contentId},
            },
            queryPaging: new QueryPaging(1, 0),
            selectedInfo: selectedInfo,
            token: CancellationToken.None
        );

        // Assert
        contents.Should().NotBeEmpty();
        var unit = contents.Single();
        unit.Id.Should().Be(contentId);
        unit.Title.Should().Be(rndContent.Title);
        unit.EngTitle.Should().Be(rndContent.EngTitle);
        unit.OriginTitle.Should().Be(rndContent.OriginTitle);
        unit.Description.Should().Be(rndContent.Description);
        unit.Type.Should().Be(rndContent.ContentType);
        unit.Status.Should().Be(rndContent.Status);
        unit.ImageId.Should().Be(rndContent.ImageId);
        unit.ImportStars.Should().Be(0); //rndContent.ImportStars);
        unit.OutSeries.Should().Be(0);
        unit.PlannedSeries.Should().Be(rndContent.PlannedSeries);
        unit.Views.Should().Be(0); //rndContent.Views); 
        unit.Country.Should().Be(rndContent.Country);
        unit.ReleasedAt.Should().BeCloseTo(rndContent.ReleasedAt!.Value, TimeSpan.FromMinutes(1));
        unit.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        unit.LastUpdateAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        unit.MinAgeLimit.Should().Be(rndContent.MinAge);
        unit.Duration.Should().Be(rndContent.Duration);
        unit.Translations.Should().BeEmpty();
        unit.Episodes.Should().BeEmpty();
        unit.Genres.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByQueryAsync__WithOffset__ShouldPass()
    {
        // Arrange
        var rndContent = RandomDataModelFactory.CreateBllContentQuery();

        // Act
        var contentId = await _contentProvider.InsertAsync(rndContent, CancellationToken.None);
        var contents = await _contentProvider.GetByQueryAsync(
            search: "",
            searchFilters: new kapace.BLL.Models.SearchFilters() { 
                ContentIds = new[] {contentId},
            },
            queryPaging: new QueryPaging(0, 1),
            selectedInfo: kapace.BLL.Enums.ContentSelectedInfo.None,
            token: CancellationToken.None
        );

        // Assert
        contents.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByQueryAsync__WithSearch__ShouldPass()
    {
        // Arrange
        var rndContent = RandomDataModelFactory.CreateBllContentQuery();

        // Act
        var contentId = await _contentProvider.InsertAsync(rndContent, CancellationToken.None);
        var contents = await _contentProvider.GetByQueryAsync(
            search: rndContent.Title,
            searchFilters: new kapace.BLL.Models.SearchFilters() { 
                ContentIds = new[] {contentId},
            },
            queryPaging: null,
            selectedInfo: kapace.BLL.Enums.ContentSelectedInfo.None,
            token: CancellationToken.None
        );

        // Assert
        contents.Should().NotBeEmpty();
        var unit = contents.Single();
        unit.Id.Should().Be(contentId);
    }

    [Fact]
    public async Task GetByQueryAsync__WithSearchPartOfTitle__ShouldPass()
    {
        // Arrange
        var rndContent = RandomDataModelFactory.CreateBllContentQuery();

        // Act
        var contentId = await _contentProvider.InsertAsync(rndContent, CancellationToken.None);
        var contents = await _contentProvider.GetByQueryAsync(
            search: rndContent.Title.Substring(0, 5),
            searchFilters: new kapace.BLL.Models.SearchFilters() { 
                ContentIds = new[] {contentId},
            },
            queryPaging: null,
            selectedInfo: kapace.BLL.Enums.ContentSelectedInfo.None,
            token: CancellationToken.None
        );

        // Assert
        contents.Should().NotBeEmpty();
        var unit = contents.Single();
        unit.Id.Should().Be(contentId);
    }
}