using CmdPalTranslator.Commands;
using CmdPalTranslator.Models;
using CmdPalTranslator.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CmdPalTranslator.Pages
{
    internal sealed partial class TranslatorSettingsPage : DynamicListPage
    {
        private readonly TranslatorSettingsService _settingsService;

        public TranslatorSettingsPage(TranslatorSettingsService settingsService)
        {
            _settingsService = settingsService;
            _settingsService.SettingsChanged += OnSettingsChanged;

            Name = "Translation Settings";
            Title = "Target Language";
            Icon = new IconInfo("\uE713");
            ShowDetails = true;
        }

        public override void UpdateSearchText(string oldSearch, string newSearch)
        {
            RaiseItemsChanged();
        }

        public override IListItem[] GetItems()
        {
            LanguageOption currentLanguage = _settingsService.TargetLanguage;
            IEnumerable<LanguageOption> languages = LanguageCatalog.All
                .Where(language => !string.Equals(language.Id, LanguageCatalog.AutoDetect.Id, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string keyword = SearchText.Trim();
                languages = languages.Where(language =>
                    language.Matches(keyword)
                    || language.Id.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || language.DisplayName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            List<IListItem> items =
            [
                new ListItem(new NoOpCommand())
                {
                    Title = "Current target language",
                    Subtitle = $"{currentLanguage.DisplayName} ({currentLanguage.Id})",
                    Icon = new IconInfo("\uE909"),
                    Details = new Details
                    {
                        Title = "How target language works",
                        Body = "New translations use this language unless you append `-> languageCode` in the query.\nExample: `hello world -> ja`",
                    },
                },
            ];

            items.AddRange(languages.Select(language => BuildLanguageItem(language, currentLanguage)));
            return [.. items];
        }

        private ListItem BuildLanguageItem(LanguageOption language, LanguageOption currentLanguage)
        {
            bool isCurrent = string.Equals(language.Id, currentLanguage.Id, StringComparison.OrdinalIgnoreCase);
            string title = isCurrent ? $"{language.DisplayName} (Current)" : language.DisplayName;
            string subtitle = isCurrent
                ? $"{language.Id} · Used when no `-> languageCode` override is specified"
                : $"{language.Id} · Set as the translation target";

            return new ListItem(new SetTargetLanguageCommand(_settingsService, language))
            {
                Title = title,
                Subtitle = subtitle,
                Icon = new IconInfo(isCurrent ? "\uE73A" : "\uE739"),
                Details = new Details
                {
                    Title = $"{language.DisplayName} ({language.Id})",
                    Body = isCurrent
                        ? $"This is the current target language.\nExample query without override: `hello world`"
                        : $"Set this as the target language for new translations.\nExample query with explicit override: `hello world -> {language.Id}`",
                },
            };
        }

        private void OnSettingsChanged(object? sender, EventArgs e)
        {
            RaiseItemsChanged();
        }
    }
}
