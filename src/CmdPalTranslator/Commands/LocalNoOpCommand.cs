using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalTranslator.Commands
{
    internal sealed partial class LocalNoOpCommand : InvokableCommand
    {
        public override string Name => "Stay";

        public override ICommandResult Invoke() => CommandResult.KeepOpen();
    }
}
