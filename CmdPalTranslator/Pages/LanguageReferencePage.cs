using CmdPalTranslator.Commands;
using CmdPalTranslator.Models;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Linq;

namespace CmdPalTranslator.Pages
{
    internal sealed partial class LanguageReferencePage : ListPage
    {
        public LanguageReferencePage()
        {
            Name = "Supported Languages";
            Icon = new IconInfo("\uE909");
            ShowDetails = true;
        }

        public override IListItem[] GetItems()
        {
            return LanguageCatalog.All
                .Select(language => new ListItem(new LocalCopyTextCommand(language.Id, $"Copied `{language.Id}`"))
                {
                    Title = language.DisplayName,
                    Subtitle = $"{language.Id} · Example: hello -> {language.Id}",
                    Details = new Details
                    {
                        Title = $"{language.DisplayName} ({language.Id})",
                        Body = $"Use `{language.Id}` as the target language suffix.\nExample query: `hello world -> {language.Id}`",
                    },
                })
                .ToArray<IListItem>();
        }
    }
}
