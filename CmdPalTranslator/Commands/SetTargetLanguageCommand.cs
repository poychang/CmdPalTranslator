using CmdPalTranslator.Models;
using CmdPalTranslator.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;

namespace CmdPalTranslator.Commands
{
    internal sealed partial class SetTargetLanguageCommand(
        TranslatorSettingsService settingsService,
        LanguageOption language) : InvokableCommand
    {
        public override string Name => "Set target language";

        public override ICommandResult Invoke()
        {
            try
            {
                bool changed = settingsService.SetTargetLanguage(language);
                string message = changed
                    ? $"Target language set to {language.DisplayName}"
                    : $"{language.DisplayName} is already the target language";

                return CommandResult.ShowToast(new ToastArgs()
                {
                    Message = message,
                    Result = CommandResult.KeepOpen(),
                });
            }
            catch (Exception ex)
            {
                return CommandResult.ShowToast(new ToastArgs()
                {
                    Message = $"Failed to save target language: {ex.Message}",
                    Result = CommandResult.KeepOpen(),
                });
            }
        }
    }
}
