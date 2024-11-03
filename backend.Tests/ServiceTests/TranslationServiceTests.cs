using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.Tests.Fixtures;
using backend.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Tests.ServiceTests;

public class TranslationServiceTests : IClassFixture<MainFixture>
{
    private readonly MainFixture _mainFixture;
    private static readonly Bogus.Faker _faker = new();
    private readonly CancellationToken token = CancellationToken.None;
    public TranslationServiceTests(MainFixture mainFixture)
    {
        _mainFixture = mainFixture;
    }

    [Fact]
    public async Task Query_WithEpisode_ShouldPass()
    {
        // Arrange
        var provider = _mainFixture.ServiceProvider;
        var translationsService = provider.GetService<ITranslationService>() ?? throw new NullReferenceException(nameof(ITranslationService));
        var translatorsRepository = provider.GetService<ITranslatorsRepository>() ?? throw new NullReferenceException(nameof(ITranslatorsRepository));
        var episodeRepository = provider.GetService<IEpisodeRepository>() ?? throw new NullReferenceException(nameof(IEpisodeRepository));
        var contentRepository = provider.GetService<IContentRepository>() ?? throw new NullReferenceException(nameof(IContentRepository));

        var rndString = TestHelpers.GetRandomString();
        var rndInt = TestHelpers.GetRandomInt();
        var fakeInsertModel = RandomDataModelFactory.CreateInsertContentQuery();

        // Act
        var contentId = await contentRepository.InsertAsync(fakeInsertModel, token);
        var translatorId = await translatorsRepository.InsertAsync(Translator.CreateInsertModel(rndString, rndString), token);
        var episodeId = await episodeRepository.InsertAsync(
            Episode.CreateInsertModel(
                contentId,
                rndInt,
                rndString,
                rndInt,
                rndInt),
            token);
        //var translationId = await translationsService.InsertAsync();

        // Assert
        var episodeTranslations = await translationsService.GetByEpisodeAsync(
            contentId,
            episodeId,
            translatorId,
            null,
            token);

        //episodeTranslations.Should().NotBeEmpty();
        //var episodeTranslation = episodeTranslations.First();
    }
}