// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CmdPalTranslator.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalTranslator;

public partial class CmdPalTranslatorCommandsProvider : CommandProvider
{
    private readonly TranslatorService _translatorService;
    private readonly ICommandItem[] _commands;

    internal CmdPalTranslatorCommandsProvider(TranslatorService translatorService)
    {
        _translatorService = translatorService;

        DisplayName = "Translator";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");

        _commands = [
            new CommandItem(new CmdPalTranslatorPage(_translatorService))
            {
                Title = "Translator",
                Subtitle = "Instantly translate text and switch between Bing and Google translate.",
            },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

    public override void Dispose()
    {
        _translatorService.Dispose();
        base.Dispose();
    }
}
