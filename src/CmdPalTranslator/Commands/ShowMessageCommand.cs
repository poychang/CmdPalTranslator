using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Runtime.InteropServices;

namespace CmdPalTranslator.Commands
{
    internal sealed partial class ShowMessageCommand : InvokableCommand
    {
        public override string Name => "Show message";
        public override IconInfo Icon => new("\uE8A7");

        public override CommandResult Invoke()
        {
            // 0x00001000 is MB_SYSTEMMODAL, which will display the message box on top of other windows.
            _ = MessageBox(0, "I came from the Command Palette", "What's up?", 0x00001000);
            return CommandResult.KeepOpen();
        }

        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int MessageBox(IntPtr hWnd, string text, string caption, uint type);
    }

    internal sealed partial class ToastCommand(string message, MessageState state = MessageState.Info) : InvokableCommand
    {
        public override ICommandResult Invoke()
        {
            var t = new ToastStatusMessage(new StatusMessage()
            {
                Message = message,
                State = state,
            });
            t.Show();

            return CommandResult.KeepOpen();
        }
    }
}
