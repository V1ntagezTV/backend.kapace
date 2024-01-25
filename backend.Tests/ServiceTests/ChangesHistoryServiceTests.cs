using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Services.Interfaces;
using backend.Tests.Fixtures;
using backend.Tests.Helpers;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Tests.ServiceTests;


public class ChangesHistoryServiceTests : IClassFixture<MainFixture>
{
    private readonly CancellationToken _token = CancellationToken.None;
    private readonly Faker _faker = new();

    private readonly IChangesHistoryService _changesHistoryService;
    private readonly IContentService _contentService;

    public ChangesHistoryServiceTests(MainFixture mainFixture)
    {
        _changesHistoryService = mainFixture.ServiceProvider.GetService<IChangesHistoryService>() 
                                 ?? throw new NullReferenceException();
        _contentService = mainFixture.ServiceProvider.GetService<IContentService>() 
                          ?? throw new NullReferenceException();
    }

    [Fact]
    public async Task CreateContentChanges_Create_ShouldPass()
    {
        // Arrange
        var contentId = _faker.Random.Number(min: 0);
        var newHistoryUnit = RandomDataModelFactory.CreateHistoryUnitWithContent(contentId);

        // Act
        var changesId = await _changesHistoryService.InsertChangesAsync(newHistoryUnit, CancellationToken.None);
        
        // Assert
        var query = (await _changesHistoryService
            .QueryAsync(new[] { changesId }, CancellationToken.None))
                .Single();

        query.Id.Should().Be(changesId);
        query.HistoryType.Should().Be(HistoryType.Content);
        query.CreatedBy.Should().Be(newHistoryUnit.CreatedBy);
        query.CreatedAt.Should().BeCloseTo(newHistoryUnit.CreatedAt, TimeSpan.FromSeconds(5));
        query.ApprovedBy.Should().Be(newHistoryUnit.ApprovedBy);
        query.ApprovedAt.Should().Be(newHistoryUnit.ApprovedAt);

        var changes = (HistoryUnit.JsonContentChanges)query.Changes;
        var updates = (HistoryUnit.JsonContentChanges)newHistoryUnit.Changes;
        changes.ImageId.Should().Be(updates.ImageId);
        changes.Title.Should().Be(updates.Title);
        changes.EngTitle.Should().Be(updates.EngTitle);
        changes.OriginTitle.Should().Be(updates.OriginTitle);
        changes.Description.Should().Be(updates.Description);
        changes.Country.Should().Be(updates.Country);
        changes.ContentType.Should().Be(updates.ContentType);
        changes.Genres.Should().Be(updates.Genres);
        changes.Duration.Should().Be(updates.Duration);
        changes.ReleasedAt.Should().Be(updates.ReleasedAt);
        changes.PlannedSeries.Should().Be(updates.PlannedSeries);
        changes.MinAge.Should().Be(updates.MinAge);
    }

    [Fact]
    public async Task ApproveContentChanges_CreateAndApprove_ShouldPass()
    {
        // Arrange
        var contentId = _faker.Random.Number(min: 0);
        var approverId = _faker.Random.Number(min: 0);
        var newHistoryUnit = RandomDataModelFactory.CreateHistoryUnitWithContent(contentId);

        // Act
        var historyId = await _changesHistoryService.InsertChangesAsync(newHistoryUnit, CancellationToken.None);
        await _changesHistoryService.ApproveAsync(historyId, approverId, CancellationToken.None);

        // Assert
        var query = (await _changesHistoryService
                .QueryAsync(new[] { historyId }, CancellationToken.None))
            .Single();
        var content = (await _contentService
                .GetByQueryAsync(
                    new SearchFilters()
                    {
                        ContentIds = new[] { historyId },
                    },
                    ContentSelectedInfo.None,
                    _token))
            .Single();

        query.Id.Should().Be(historyId);
        query.HistoryType.Should().Be(HistoryType.Content);
        query.CreatedBy.Should().Be(newHistoryUnit.CreatedBy);
        query.CreatedAt.Should().BeCloseTo(newHistoryUnit.CreatedAt, TimeSpan.FromSeconds(5));
        query.ApprovedBy.Should().Be(approverId);
        query.ApprovedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        
        var updates = (HistoryUnit.JsonContentChanges)newHistoryUnit.Changes;
        content.ImageId.Should().Be(updates.ImageId);
        content.Title.Should().Be(updates.Title);
        content.EngTitle.Should().Be(updates.EngTitle);
        content.OriginTitle.Should().Be(updates.OriginTitle);
        content.Description.Should().Be(updates.Description);
        content.Country.Should().Be(updates.Country);
        content.Type.Should().Be(updates.ContentType);
        content.Genres.Should().BeEmpty();
        content.Duration.Should().Be(updates.Duration);
        content.ReleasedAt.Should().BeCloseTo(updates.ReleasedAt!.Value, TimeSpan.FromSeconds(1));
        content.PlannedSeries.Should().Be(updates.PlannedSeries);
        content.MinAgeLimit.Should().Be(updates.MinAge);
    }
}