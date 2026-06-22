using CmdPalTranslator.Commands;
using CmdPalTranslator.Core.Models;
using CmdPalTranslator.Core.Services;
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

            Name = LocalizationService.Instance.Get("Page.LanguageReference.Name");
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
                        Body = LocalizationService.Instance.Get("Page.LanguageReference.Item.Details.Body", _translateOperator, language.Id),
                        Metadata = [
                            new DetailsElement()
                            {
                                Key = LocalizationService.Instance.Get("Page.LanguageReference.Item.Details.Example.Key"),
                                Data = new DetailsLink() { Text = $"hello {_translateOperator} {language.Id}" },
                            },
                        ],
                    },
                    Tags = [new Tag(language.Id), new Tag($"hello {_translateOperator} {language.Id}")],
                })];
        }
    }
}
