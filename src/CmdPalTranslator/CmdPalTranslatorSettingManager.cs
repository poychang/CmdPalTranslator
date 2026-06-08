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
        private readonly CmdPalTranslatorSettingService _settingService;

        public CmdPalTranslatorSettingManager(CmdPalTranslatorSettingService? settingService = null)
        {
            _settingService = settingService ?? new CmdPalTranslatorSettingService();
            _settingService.SettingsChanged += (_, _) => SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public ICommandSettings CommandSettings => _settings;

        public event EventHandler? SettingsChanged;

        public LanguageOption TargetLanguage => _settingService.TargetLanguage;

        public string PreferredProviderId => _settingService.PreferredProviderId;

        public bool SetTargetLanguage(LanguageOption language) => _settingService.SetTargetLanguage(language);

        public bool SetPreferredProvider(string providerId) => _settingService.SetPreferredProvider(providerId);
    }
}
