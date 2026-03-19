using System;
using System.Collections.Generic;

namespace CmdPalTranslator.Models
{
    internal sealed record ParsedTranslationQuery(
        string SourceText,
        LanguageOption SourceLanguage,
        LanguageOption TargetLanguage,
        bool HasExplicitTargetLanguage);

    internal sealed record TranslationEntry(
        string Title,
        string Subtitle,
        string CopyText,
        string? Description = null,
        string? Category = null);

    internal sealed record TranslationResponse(
        string ProviderId,
        string ProviderDisplayName,
        string SourceLanguage,
        string TargetLanguage,
        string SourceText,
        IReadOnlyList<TranslationEntry> Entries,
        Uri? WebUri = null);
}
