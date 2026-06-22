using CmdPalTranslator.Core.Models;
using CmdPalTranslator.Core.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;

namespace CmdPalTranslator
{
    internal sealed partial class CmdPalTranslatorSettingManager
    {
        private readonly Settings _settings = new();
        private readonly CmdPalTranslatorSettingService _translatorSettingService;

        public CmdPalTranslatorSettingManager(CmdPalTranslatorSettingService? translatorSettingService = null)
        {
            _translatorSettingService = translatorSettingService ?? new CmdPalTranslatorSettingService();

            // Load the display language before initializing any settings UI strings.
            LocalizationService.Instance.Load(_translatorSettingService.DisplayLanguageId);

            InitializePreferredProviderSettings();
            InitializeTranslateOperatorSettings();
            InitializeDisplayLanguageSettings();
            _translatorSettingService.SettingsChanged += OnServiceSettingsChanged;
        }

        public ICommandSettings CommandSettings => _settings;

        public event EventHandler? SettingsChanged;

        public LanguageOption TargetLanguage => _translatorSettingService.TargetLanguage;

        public string TranslateOperator => _translatorSettingService.TranslateOperator;

        public bool SetTargetLanguage(LanguageOption language) => _translatorSettingService.SetTargetLanguage(language);

        public bool SetTranslateOperator(string translateOperator) => _translatorSettingService.SetTranslateOperator(translateOperator);

        private void OnServiceSettingsChanged(object? sender, EventArgs e)
        {
            LocalizationService.Instance.Load(_translatorSettingService.DisplayLanguageId);
            ApplyLocalization();
            ApplyPreferredProviderSettingValue(_translatorSettingService.PreferredProviderId);
            ApplyTranslateOperatorSettingValue(_translatorSettingService.TranslateOperator);
            ApplyDisplayLanguageSettingValue(_translatorSettingService.DisplayLanguageId);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ApplyLocalization()
        {
            _preferredProviderSetting.Label = LocalizationService.Instance.Get("Settings.PreferredProvider.Label");
            _preferredProviderSetting.Description = LocalizationService.Instance.Get("Settings.PreferredProvider.Description");
            _translateOperatorSetting.Label = LocalizationService.Instance.Get("Settings.TranslateOperator.Label");
            _translateOperatorSetting.Description = LocalizationService.Instance.Get("Settings.TranslateOperator.Description");
            _displayLanguageSetting.Label = LocalizationService.Instance.Get("Settings.DisplayLanguage.Label");
            _displayLanguageSetting.Description = LocalizationService.Instance.Get("Settings.DisplayLanguage.Description");
        }
    }
}
