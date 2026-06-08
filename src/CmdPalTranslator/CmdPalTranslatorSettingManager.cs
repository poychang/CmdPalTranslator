using CmdPalTranslator.Models;
using CmdPalTranslator.Services;
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
            _translatorSettingService.SettingsChanged += OnServiceSettingsChanged;
        }

        public ICommandSettings CommandSettings => _settings;

        public event EventHandler? SettingsChanged;

        public LanguageOption TargetLanguage => _translatorSettingService.TargetLanguage;

        public bool SetTargetLanguage(LanguageOption language) => _translatorSettingService.SetTargetLanguage(language);

        private void OnServiceSettingsChanged(object? sender, EventArgs e)
        {
            ApplyPreferredProviderSettingValue(_translatorSettingService.PreferredProviderId);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
