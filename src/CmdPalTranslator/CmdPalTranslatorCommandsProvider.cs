// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CmdPalTranslator.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalTranslator;

public partial class CmdPalTranslatorCommandsProvider : CommandProvider
{
    private readonly CmdPalTranslatorSettingManager _settingsManager;
    private readonly TranslatorService _translatorService;
    private readonly ICommandItem[] _commands;

    internal CmdPalTranslatorCommandsProvider(TranslatorService translatorService, CmdPalTranslatorSettingManager settingsManager)
    {
        _translatorService = translatorService;
        _settingsManager = settingsManager;

        DisplayName = "Translator";
        Icon = IconHelpers.FromRelativePath("Assets\\icons\\StoreLogo.png");
        Settings = _settingsManager.CommandSettings;

        _commands = [
            new CommandItem(new CmdPalTranslatorPage(_settingsManager, _translatorService))
            {
                Title = "Translator",
                Subtitle = "Instantly translate text.",
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
