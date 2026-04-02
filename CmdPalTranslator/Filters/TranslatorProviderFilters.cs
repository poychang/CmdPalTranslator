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
        private readonly TranslatorSettingsService _settingsService;
        private string _currentFilterId;

        public TranslatorProviderFilters(TranslatorService translatorService)
        {
            _translatorService = translatorService;
            _settingsService = translatorService.Settings;
            _currentFilterId = _settingsService.PreferredProviderId;
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
                _settingsService.SetPreferredProvider(value);
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
