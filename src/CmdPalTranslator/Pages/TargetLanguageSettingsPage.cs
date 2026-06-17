using CmdPalTranslator.Commands;
using CmdPalTranslator.Core.Models;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CmdPalTranslator.Pages
{
    internal sealed partial class TargetLanguageSettingsPage : DynamicListPage
    {
        private readonly CmdPalTranslatorSettingManager _translatorSettingManager;

        public TargetLanguageSettingsPage(CmdPalTranslatorSettingManager translatorSettingManager)
        {
            _translatorSettingManager = translatorSettingManager;
            _translatorSettingManager.SettingsChanged += OnSettingsChanged;

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
            LanguageOption currentLanguage = _translatorSettingManager.TargetLanguage;
            string translateOperator = _translatorSettingManager.TranslateOperator;
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
                        Body = $"New translations use this language unless you append `{translateOperator} languageCode` in the query.\nExample: `hello world {translateOperator} ja`",
                    },
                },
            ];

            items.AddRange(languages.Select(language => BuildLanguageItem(language, currentLanguage, translateOperator)));
            return [.. items];
        }

        private ListItem BuildLanguageItem(LanguageOption language, LanguageOption currentLanguage, string translateOperator)
        {
            bool isCurrent = string.Equals(language.Id, currentLanguage.Id, StringComparison.OrdinalIgnoreCase);
            string title = isCurrent ? $"{language.DisplayName} (Current)" : language.DisplayName;
            string subtitle = isCurrent ? string.Empty : $"Set as the target";

            return new ListItem(new SetTargetLanguageCommand(_translatorSettingManager, language))
            {
                Title = title,
                Subtitle = subtitle,
                Icon = new IconInfo(isCurrent ? "\uE73A" : "\uE739"),
                Details = new Details
                {
                    Title = $"{language.DisplayName} ({language.Id})",
                    Body = isCurrent
                        ? $"This is the current target language.\n Used when no `{translateOperator} languageCode` override is specified.\n Example query without override: `hello world`"
                        : $"Set this as the target language for new translations.\n Example query with explicit override: `hello world {translateOperator} {language.Id}`",
                },
                Tags = [new Tag(language.Id)],
            };
        }

        private void OnSettingsChanged(object? sender, EventArgs e)
        {
            RaiseItemsChanged();
        }
    }
}
