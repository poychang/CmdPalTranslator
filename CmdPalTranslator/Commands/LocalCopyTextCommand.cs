using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.ApplicationModel.DataTransfer;

namespace CmdPalTranslator.Commands
{
    internal sealed partial class LocalCopyTextCommand(string text, string? successMessage = null) : InvokableCommand
    {
        public override string Name => "Copy";

        public override ICommandResult Invoke()
        {
            DataPackage package = new();
            package.SetText(text);
            Clipboard.SetContent(package);
            Clipboard.Flush();

            return CommandResult.ShowToast(new ToastArgs()
            {
                Message = successMessage ?? "Copied to clipboard",
                Result = CommandResult.KeepOpen(),
            });
        }
    }
}
