﻿using backend.kapace.BLL.Enums;

namespace backend.kapace.BLL.Models;

public record TranslatesView(
    TranslatesView.Translator[] Translators,
    TranslatesView.Episode[] Episodes)
{
    public record Episode(
        long Id,
        string Title,
        int Number,
        int Views,
        double Stars,
        EpisodeTranslation[] Translations);
    
    public record EpisodeTranslation(
        long Id,
        long EpisodeId,
        Language Lang,
        string Link,
        TranslationType TranslationType,
        DateTimeOffset CreatedAt,
        long CreatedBy,
        int Quality);

    public record Translator(long Id, string? Name);
}