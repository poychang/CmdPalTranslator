using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;

namespace CmdPalTranslator
{
    internal sealed partial class CmdPalTranslatorSettingManager
    {
        private const string PreferredProviderSettingId = "preferredProvider";

        private static readonly List<ChoiceSetSetting.Choice> PreferredProviderChoices =
        [
            new("Bing", "bing"),
            new("Google", "google"),
            new("Aliyun", "aliyun"),
        ];

        private ChoiceSetSetting _preferredProviderSetting = null!;

        public string PreferredProviderId => _translatorSettingService.PreferredProviderId;

        public bool SetPreferredProvider(string providerId)
        {
            bool changed = _translatorSettingService.SetPreferredProvider(providerId);
            if (changed)
            {
                ApplyPreferredProviderSettingValue(_translatorSettingService.PreferredProviderId);
            }

            return changed;
        }

        private void InitializePreferredProviderSettings()
        {
            _preferredProviderSetting = new ChoiceSetSetting(PreferredProviderSettingId, PreferredProviderChoices)
            {
                Label = LocalizationService.Instance.Get("Settings.PreferredProvider.Label"),
                Description = LocalizationService.Instance.Get("Settings.PreferredProvider.Description"),
            };

            _settings.Add(_preferredProviderSetting);
            ApplyPreferredProviderSettingValue(_translatorSettingService.PreferredProviderId);
            _settings.SettingsChanged += OnCommandSettingsChanged;
        }

        private void OnCommandSettingsChanged(object sender, Settings args)
        {
            string? selectedProviderId = _settings.GetSetting<string>(PreferredProviderSettingId);
            if (!string.IsNullOrWhiteSpace(selectedProviderId))
            {
                _translatorSettingService.SetPreferredProvider(selectedProviderId);
            }
        }

        private void ApplyPreferredProviderSettingValue(string providerId)
        {
            if (!string.Equals(_preferredProviderSetting.Value, providerId, StringComparison.OrdinalIgnoreCase))
            {
                _preferredProviderSetting.Value = providerId;
            }
        }
    }
}
