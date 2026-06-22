// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CmdPalTranslator.Commands;
using CmdPalTranslator.Core.Models;
using CmdPalTranslator.Core.Providers;
using CmdPalTranslator.Core.Services;
using CmdPalTranslator.Pages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CmdPalTranslator;

internal sealed partial class CmdPalTranslatorPage : DynamicListPage, IDisposable
{
    private readonly CmdPalTranslatorSettingManager _translatorSettingManager;
    private readonly TranslatorService _translatorService;
    private CancellationTokenSource? _debounceCts;
    private const int DebounceDelayMs = 300;

    public CmdPalTranslatorPage(CmdPalTranslatorSettingManager translatorSettingManager, TranslatorService translatorService)
    {
        _translatorSettingManager = translatorSettingManager;
        _translatorSettingManager.SettingsChanged += (_, _) => RaiseItemsChanged();

        _translatorService = translatorService;

        Icon = IconHelpers.FromRelativePath("Assets\\icons\\StoreLogo.png");
        Title = LocalizationService.Instance.Get("Page.Main.Title");
        Name = LocalizationService.Instance.Get("Page.Main.Name");
        ShowDetails = true;
    }

    public override async void UpdateSearchText(string oldSearch, string newSearch)
    {
        CancellationTokenSource newDebounceCts = new();
        CancellationTokenSource? previousCts = Interlocked.Exchange(ref _debounceCts, newDebounceCts);
        previousCts?.Cancel();
        previousCts?.Dispose();

        var token = newDebounceCts.Token;

        try
        {
            await Task.Delay(DebounceDelayMs, token);
            RaiseItemsChanged();
        }
        catch (TaskCanceledException)
        {
            // Debounce cancelled by newer input; ignore.
        }
    }

    public override IListItem[] GetItems()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return BuildHelpItems();
        }

        ParsedTranslationQuery query = TranslatorService.ParseQuery(
            SearchText,
            _translatorSettingManager.TargetLanguage,
            _translatorSettingManager.TranslateOperator);
        ITranslatorProvider provider = _translatorService.GetProvider(GetSelectedProviderId());

        try
        {
            TranslationResponse translation = provider.Translate(query, default);
            return [.. translation.Entries
                .Select(entry => BuildTranslationItem(entry, translation, query))
                .Cast<IListItem>()];
        }
        catch (Exception ex)
        {
            string failedTitle = ex is TaskCanceledException or OperationCanceledException
                ? LocalizationService.Instance.Get("Page.Main.Error.TranslationTimedOut")
                : LocalizationService.Instance.Get("Page.Main.Error.TranslationFailed", provider.DisplayName);
            string failedMessage = ex is TaskCanceledException or OperationCanceledException
                ? LocalizationService.Instance.Get("Page.Main.Error.RequestTimedOut")
                : ex.Message;

            return
            [
                new ListItem(new LocalNoOpCommand())
                {
                    Title = failedTitle,
                    Subtitle = failedMessage,
                    Details = new Details
                    {
                        Title = failedTitle,
                        Body = LocalizationService.Instance.Get("Page.Main.Error.SomethingWrong"),
                        Metadata = [
                            new DetailsElement()
                            {
                                Key = LocalizationService.Instance.Get("Page.Main.Error.Details.Query.Key"),
                                Data = new DetailsLink() { Text = query.SourceText },
                            },
                            new DetailsElement()
                            {
                                Key = LocalizationService.Instance.Get("Page.Main.Error.Details.Target.Key"),
                                Data = new DetailsLink() { Text = query.TargetLanguage.DisplayName },
                            },
                            new DetailsElement()
                            {
                                Key = LocalizationService.Instance.Get("Page.Main.Error.Details.FailedMessage.Key"),
                                Data = new DetailsLink() { Text = failedMessage },
                            },
                        ],
                    },
                },
            ];
        }
    }

    private IListItem[] BuildHelpItems()
    {
        LanguageOption defaultTarget = _translatorSettingManager.TargetLanguage;
        string translateOperator = _translatorSettingManager.TranslateOperator;

        return
        [
            new ListItem(new NoOpCommand())
            {
                Title = LocalizationService.Instance.Get("Page.Main.Help.Title"),
                Subtitle = LocalizationService.Instance.Get("Page.Main.Help.Subtitle"),
                Icon = new IconInfo("\uE721"),
            },
            new ListItem(new TargetLanguageSettingsPage(_translatorSettingManager))
            {
                Title = LocalizationService.Instance.Get("Page.Main.Help.TargetLanguage.Title"),
                Subtitle = LocalizationService.Instance.Get("Page.Main.Help.TargetLanguage.Subtitle", defaultTarget.DisplayName, defaultTarget.Id),
                Icon = new IconInfo("\uE713"),
                Details = new Details
                {
                    Title = LocalizationService.Instance.Get("Page.Main.Help.TargetLanguage.Details.Title"),
                    Body = LocalizationService.Instance.Get("Page.Main.Help.TargetLanguage.Details.Body", translateOperator),
                },
            },
            new ListItem(new LanguageReferencePage(translateOperator))
            {
                Title = LocalizationService.Instance.Get("Page.Main.Help.SupportedLanguages.Title"),
                Subtitle = LocalizationService.Instance.Get("Page.Main.Help.SupportedLanguages.Subtitle"),
                Icon = new IconInfo("\uE946"),
                Details = new Details
                {
                    Title = LocalizationService.Instance.Get("Page.Main.Help.SupportedLanguages.Details.Title"),
                    Body = LocalizationService.Instance.Get("Page.Main.Help.SupportedLanguages.Details.Body"),
                    Metadata = [
                        new DetailsElement()
                        {
                            Key = LocalizationService.Instance.Get("Page.Main.Help.SupportedLanguages.Details.TargetLanguages.Key"),
                            Data = new DetailsTags
                            {
                                Tags = [.. LanguageCatalog.All
                                    .Select(l => l.DisplayName)
                                    .Select(t => new Tag(t))],
                            },
                        },
                        new DetailsElement()
                        {
                            Key = LocalizationService.Instance.Get("Page.Main.Help.SupportedLanguages.Details.SpecifyTarget.Key"),
                            Data = new DetailsLink() { Text = LocalizationService.Instance.Get("Page.Main.Help.SupportedLanguages.Details.SpecifyTarget.Text", translateOperator) },
                        },
                    ],
                },
            },
            // ------------------------------------------------------------
            // Test commands to show the Command Palette's capabilities
            // ------------------------------------------------------------
            //new ListItem(new ShowMessageCommand()),
            //new ListItem(new OpenUrlCommand("https://learn.microsoft.com/windows/powertoys/command-palette/adding-commands"))
            //{
            //    Title = "Open the Command Palette documentation",
            //},
            //new ListItem(new NoOpCommand())
            //{
            //    Title = "Do nothing command"
            //},
        ];
    }

    private static ListItem BuildTranslationItem(TranslationEntry entry, TranslationResponse response, ParsedTranslationQuery query)
    {
        string subtitle = string.IsNullOrWhiteSpace(entry.Subtitle)
            ? response.ProviderDisplayName
            : $"{entry.Subtitle} · {response.ProviderDisplayName}";

        List<CommandContextItem> moreCommands =
        [
            new CommandContextItem(new LocalCopyTextCommand(query.SourceText, LocalizationService.Instance.Get("Page.Main.Item.CopiedSourceText")))
            {
                Title = LocalizationService.Instance.Get("Page.Main.Item.CopySourceText.Title"),
            },
        ];

        if (response.WebUri is not null)
        {
            moreCommands.Add(new CommandContextItem(new OpenUrlCommand(response.WebUri.ToString()))
            {
                Title = LocalizationService.Instance.Get("Page.Main.Item.OpenIn.Title", response.ProviderDisplayName),
            });
        }

        return new ListItem(new LocalCopyTextCommand(entry.CopyText, LocalizationService.Instance.Get("Page.Main.Item.CopiedTranslation")))
        {
            Title = entry.Title,
            Subtitle = subtitle,
            MoreCommands = [.. moreCommands],
            Details = new Details
            {
                Title = entry.Title,
                Body = entry.Description ?? $"{entry.Title}\n{query.SourceText}",
                Metadata =
                [
                    new DetailsElement()
                    {
                        Key = LocalizationService.Instance.Get("Page.Main.Item.Details.Provider.Key"),
                        Data = new DetailsLink() { Text = response.ProviderDisplayName },
                    },
                    new DetailsElement()
                    {
                        Key = LocalizationService.Instance.Get("Page.Main.Item.Details.LanguagePair.Key"),
                        Data = new DetailsLink() { Text = LocalizationService.Instance.Get("Page.Main.Item.Details.LanguagePair.Text", LanguageCatalog.ToDisplayName(response.SourceLanguage), LanguageCatalog.ToDisplayName(response.TargetLanguage)) },
                    },
                    new DetailsElement()
                    {
                        Key = LocalizationService.Instance.Get("Page.Main.Item.Details.Category.Key"),
                        Data = new DetailsLink() { Text = entry.Category ?? LocalizationService.Instance.Get("Page.Main.Item.Details.DefaultCategory") },
                    },
                ],
            },
        };
    }

    private string GetSelectedProviderId()
    {
        if (!string.IsNullOrWhiteSpace(_translatorSettingManager.PreferredProviderId))
        {
            return _translatorSettingManager.PreferredProviderId;
        }

        return TranslatorService.DefaultProviderId;
    }

    public void Dispose()
    {
        CancellationTokenSource? cts = Interlocked.Exchange(ref _debounceCts, null);
        cts?.Cancel();
        cts?.Dispose();
    }
}
