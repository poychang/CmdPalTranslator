// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CmdPalTranslator.Pages;
using CmdPalTranslator.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalTranslator;

public partial class CmdPalTranslatorCommandsProvider : CommandProvider
{
    private readonly TranslatorService _translatorService;
    private readonly CmdPalTranslatorPage _translatorPage;
    private readonly LanguageReferencePage _languageReferencePage;
    private readonly ICommandItem[] _commands;

    internal CmdPalTranslatorCommandsProvider(TranslatorService translatorService)
    {
        _translatorService = translatorService;
        _languageReferencePage = new LanguageReferencePage();
        _translatorPage = new CmdPalTranslatorPage(_translatorService, _languageReferencePage);

        DisplayName = "Translator";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        
        _commands = [
                        new CommandItem(_translatorPage)
            {
                Title = "Translator",
                Subtitle = "即時翻譯文字，並用篩選器切換 Bing、Google",
            },
            new CommandItem(_languageReferencePage)
            {
                Title = "Supported Languages",
                Subtitle = "檢視支援語言代碼與 `text -> lang` 的查詢語法",
            },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

    public new void Dispose()
    {
        _translatorService.Dispose();
    }
}
