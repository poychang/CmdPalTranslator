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

            Name = LocalizationService.Instance.Get("Page.TargetLanguage.Name");
            Title = LocalizationService.Instance.Get("Page.TargetLanguage.Title");
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
                    Title = LocalizationService.Instance.Get("Page.TargetLanguage.Current.Title"),
                    Subtitle = LocalizationService.Instance.Get("Page.TargetLanguage.Current.Subtitle", currentLanguage.DisplayName, currentLanguage.Id),
                    Icon = new IconInfo("\uE909"),
                    Details = new Details
                    {
                        Title = LocalizationService.Instance.Get("Page.TargetLanguage.Current.Details.Title"),
                        Body = LocalizationService.Instance.Get("Page.TargetLanguage.Current.Details.Body", translateOperator),
                    },
                },
            ];

            items.AddRange(languages.Select(language => BuildLanguageItem(language, currentLanguage, translateOperator)));
            return [.. items];
        }

        private ListItem BuildLanguageItem(LanguageOption language, LanguageOption currentLanguage, string translateOperator)
        {
            bool isCurrent = string.Equals(language.Id, currentLanguage.Id, StringComparison.OrdinalIgnoreCase);

            return new ListItem(new SetTargetLanguageCommand(_translatorSettingManager, language))
            {
                Title = language.DisplayName,
                Icon = new IconInfo(isCurrent ? "\uE73D" : "\uE739"),
                Details = new Details
                {
                    Title = $"{language.DisplayName} ({language.Id})",
                    Body = isCurrent
                        ? LocalizationService.Instance.Get("Page.TargetLanguage.Item.IsCurrent.Body", translateOperator)
                        : LocalizationService.Instance.Get("Page.TargetLanguage.Item.IsNotCurrent.Body", translateOperator, language.Id),
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
