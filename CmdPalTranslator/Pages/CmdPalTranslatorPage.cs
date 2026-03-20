// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CmdPalTranslator.Commands;
using CmdPalTranslator.Filters;
using CmdPalTranslator.Models;
using CmdPalTranslator.Pages;
using CmdPalTranslator.Providers;
using CmdPalTranslator.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CmdPalTranslator;

internal sealed partial class CmdPalTranslatorPage : DynamicListPage
{
    private readonly TranslatorService _translatorService;

    public CmdPalTranslatorPage(TranslatorService translatorService)
    {
        _translatorService = translatorService;
        _translatorService.Settings.SettingsChanged += (_, _) => RaiseItemsChanged();

        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Translator";
        Name = "Open";
        ShowDetails = true;

        TranslatorProviderFilters filters = new(_translatorService);
        filters.PropChanged += (_, _) => RaiseItemsChanged();
        Filters = filters;
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        RaiseItemsChanged();
    }

    public override IListItem[] GetItems()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return BuildHelpItems();
        }

        ParsedTranslationQuery query = _translatorService.ParseQuery(SearchText);
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
            return
            [
                new ListItem(new LocalNoOpCommand())
                {
                    Title = $"{provider.DisplayName} translation failed",
                    Subtitle = ex.Message,
                    Details = new Details
                    {
                        Title = $"{provider.DisplayName} translation failed",
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
                                Data = new DetailsLink() { Text = ex.Message },
                            },
                        ],
                    },
                },
            ];
        }
    }

    private IListItem[] BuildHelpItems()
    {
        LanguageOption defaultTarget = _translatorService.Settings.TargetLanguage;

        return
        [
            new ListItem(new NoOpCommand())
            {
                Title = "Type text to translate",
                Subtitle = "Use the provider filter above to switch between Bing and Google.",
                Icon = new IconInfo("\uE721"),
            },
            new ListItem(new LocalCopyTextCommand("hello world -> zht", "Copied sample query"))
            {
                Title = "Specify a target language",
                Subtitle = "Append `-> languageCode`, for example `hello world -> zht`.",
                Icon = new IconInfo("\uE946"),
                Details = new Details
                {
                    Title = "Target Language Syntax",
                    Body = "Use `text -> languageCode` when you want to override the default target language.",
                    Metadata = [
                        new DetailsElement()
                        {
                            Key = "Example",
                            Data = new DetailsLink() { Text = "hello world -> zht" },
                        },
                    ],
                },
            },
            new ListItem(new LanguageReferencePage())
            {
                Title = "Browse supported languages",
                Subtitle = "Open the language reference page and copy a language code.",
                Icon = new IconInfo("\uE946"),
                Details = new Details
                {
                    Title = "Supported Language",
                    Body = "Open the language reference page to see all supported languages and their codes for both Bing and Google translators.",
                    Metadata = [
                        new DetailsElement()
                        {
                            Key = "Quick List",
                            Data = new DetailsLink() { Text = string.Join(", ", LanguageCatalog.All.Select(l => l.DisplayName)) },
                        },
                    ],
                },
            },
            new ListItem(new TranslatorSettingsPage(_translatorService.Settings))
            {
                Title = "Target language",
                Subtitle = $"{defaultTarget.DisplayName} ({defaultTarget.Id})",
                Icon = new IconInfo("\uE713"),
                Details = new Details
                {
                    Title = "Target Language",
                    Body = "Open the settings page to choose the target language used when the query does not include `-> languageCode`.",
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
        if (Filters is TranslatorProviderFilters providerFilters && !string.IsNullOrWhiteSpace(providerFilters.CurrentFilterId))
        {
            return providerFilters.CurrentFilterId;
        }

        return TranslatorService.DefaultProviderId;
    }
}
