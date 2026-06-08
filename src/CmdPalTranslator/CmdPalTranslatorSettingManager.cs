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
            _translatorSettingService.SettingsChanged += (_, _) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public ICommandSettings CommandSettings => _settings;

        public event EventHandler? SettingsChanged;

        public LanguageOption TargetLanguage => _translatorSettingService.TargetLanguage;

        public string PreferredProviderId => _translatorSettingService.PreferredProviderId;

        public bool SetTargetLanguage(LanguageOption language) => _translatorSettingService.SetTargetLanguage(language);

        public bool SetPreferredProvider(string providerId) => _translatorSettingService.SetPreferredProvider(providerId);
    }
}
