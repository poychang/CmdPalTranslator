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
        Title = "Translator";
        Name = "Open";
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
                ? "Translation timed out"
                : $"{provider.DisplayName} translation failed";
            string failedMessage = ex is TaskCanceledException or OperationCanceledException
                ? "The request timed out. Please try again later."
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
                        Body = $"Something goes wrong...",
                        Metadata = [
                            new DetailsElement()
                            {
                                Key = "Query",
                                Data = new DetailsLink() { Text = query.SourceText },
                            },
                            new DetailsElement()
                            {
                                Key = "Target",
                                Data = new DetailsLink() { Text = query.TargetLanguage.DisplayName },
                            },
                            new DetailsElement()
                            {
                                Key = "Failed Message",
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
                Title = "Type text to translate",
                Subtitle = "Use the translation provider configured in extension settings.",
                Icon = new IconInfo("\uE721"),
            },
            new ListItem(new TargetLanguageSettingsPage(_translatorSettingManager))
            {
                Title = "Target language",
                Subtitle = $"{defaultTarget.DisplayName} ({defaultTarget.Id})",
                Icon = new IconInfo("\uE713"),
                Details = new Details
                {
                    Title = "Target Language",
                    Body = $"Open the settings page to choose the target language used when the query does not include `{translateOperator} languageCode`.",
                },
            },
            new ListItem(new LanguageReferencePage(translateOperator))
            {
                Title = "Supported Languages",
                Subtitle = "Open the language reference page.",
                Icon = new IconInfo("\uE946"),
                Details = new Details
                {
                    Title = "Supported Languages",
                    Body = "Open the language reference page to see all supported languages and their codes.",
                    Metadata = [
                        new DetailsElement()
                        {
                            Key = "Target Languages",
                            Data = new DetailsTags
                            {
                                Tags = [.. LanguageCatalog.All
                                    .Select(l => l.DisplayName)
                                    .Select(t => new Tag(t))],
                            },
                        },
                        new DetailsElement()
                        {
                            Key = "Specify Target",
                            Data = new DetailsLink() { Text = $"Append {translateOperator} and the languageCode to override the default target language.\r\n\r\nExample: hello world {translateOperator} zht" },
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
            new CommandContextItem(new LocalCopyTextCommand(query.SourceText, "Copied source text"))
            {
                Title = "Copy source text",
            },
        ];

        if (response.WebUri is not null)
        {
            moreCommands.Add(new CommandContextItem(new OpenUrlCommand(response.WebUri.ToString()))
            {
                Title = $"Open in {response.ProviderDisplayName}",
            });
        }

        return new ListItem(new LocalCopyTextCommand(entry.CopyText, "Copied translation"))
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
                        Key = "Provider",
                        Data = new DetailsLink() { Text = response.ProviderDisplayName },
                    },
                    new DetailsElement()
                    {
                        Key = "Language Pair",
                        Data = new DetailsLink() { Text = $"{LanguageCatalog.ToDisplayName(response.SourceLanguage)} -> {LanguageCatalog.ToDisplayName(response.TargetLanguage)}" },
                    },
                    new DetailsElement()
                    {
                        Key = "Category",
                        Data = new DetailsLink() { Text = entry.Category ?? "Translation" },
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
