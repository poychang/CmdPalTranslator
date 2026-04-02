using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalTranslator.Commands
{
    internal sealed partial class LocalCopyTextCommand(string text, string? successMessage = null) : InvokableCommand
    {
        public override string Name => "Copy";

        public override ICommandResult Invoke()
        {
            ClipboardHelper.SetText(text);

            return CommandResult.ShowToast(new ToastArgs()
            {
                Message = successMessage ?? "Copied to clipboard",
                Result = CommandResult.KeepOpen(),
            });
        }
    }
}
