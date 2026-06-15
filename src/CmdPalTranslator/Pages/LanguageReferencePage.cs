using CmdPalTranslator.Commands;
using CmdPalTranslator.Core.Services;
using CmdPalTranslator.Models;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Linq;

namespace CmdPalTranslator.Pages
{
    internal sealed partial class LanguageReferencePage : ListPage
    {
        private readonly string _translateOperator;

        public LanguageReferencePage(string? translateOperator = null)
        {
            _translateOperator = string.IsNullOrWhiteSpace(translateOperator)
                ? TranslatorService.DefaultTranslateOperator
                : translateOperator.Trim();

            Name = "Supported Languages";
            Icon = new IconInfo("\uE909");
            ShowDetails = true;
        }

        public override IListItem[] GetItems()
        {
            return [.. LanguageCatalog.All
                .Select(language => new ListItem(new LocalCopyTextCommand($"hello {_translateOperator} {language.Id}", $"Copied `hello {_translateOperator} {language.Id}`"))
                {
                    Title = language.DisplayName,
                    //Subtitle = $"{language.Id} · Example: hello {_translateOperator} {language.Id}",
                    Details = new Details
                    {
                        Title = $"{language.DisplayName} ({language.Id})",
                        Body = $"Use `{_translateOperator} {language.Id}` as the target language suffix.",
                        Metadata = [
                            new DetailsElement()
                            {
                                Key = "Example",
                                Data = new DetailsLink() { Text = $"hello {_translateOperator} {language.Id}" },
                            },
                        ],
                    },
                    Tags = [new Tag(language.Id), new Tag($"hello {_translateOperator} {language.Id}")],
                })];
        }
    }
}
