using CmdPalTranslator.Models;

namespace CmdPalTranslator.Providers
{
    internal interface ITranslatorProvider : IDisposable
    {
        string Id { get; }

        string DisplayName { get; }

        string Description { get; }

        TranslationResponse Translate(ParsedTranslationQuery query, CancellationToken cancellationToken);

        Uri BuildWebUri(ParsedTranslationQuery query);
    }
}
