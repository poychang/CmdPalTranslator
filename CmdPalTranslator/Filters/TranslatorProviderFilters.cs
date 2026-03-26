using CmdPalTranslator.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Linq;
using Windows.Foundation;

namespace CmdPalTranslator.Filters
{
    internal sealed partial class TranslatorProviderFilters : IFilters
    {
        private readonly TranslatorService _translatorService;
        private string _currentFilterId = TranslatorService.DefaultProviderId;

        public TranslatorProviderFilters(TranslatorService translatorService)
        {
            _translatorService = translatorService;
        }

        public string CurrentFilterId
        {
            get => _currentFilterId;
            set
            {
                if (string.Equals(_currentFilterId, value, System.StringComparison.Ordinal))
                {
                    return;
                }

                _currentFilterId = value;
                PropChanged?.Invoke(this, new PropChangedEventArgs(nameof(CurrentFilterId)));
            }
        }

        public event TypedEventHandler<object, IPropChangedEventArgs>? PropChanged;

        public IFilterItem[] GetFilters()
        {
            return _translatorService.Providers
                .Select(provider => (IFilterItem)new Filter()
                {
                    Id = provider.Id,
                    Name = provider.DisplayName,
                })
                .ToArray();
        }
    }
}
