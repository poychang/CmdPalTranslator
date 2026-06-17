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

            InitializePreferredProviderSettings();
            InitializeTranslateOperatorSettings();
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
            ApplyPreferredProviderSettingValue(_translatorSettingService.PreferredProviderId);
            ApplyTranslateOperatorSettingValue(_translatorSettingService.TranslateOperator);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
