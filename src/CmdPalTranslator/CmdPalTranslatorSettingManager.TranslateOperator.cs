using CmdPalTranslator.Core.Services;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;

namespace CmdPalTranslator
{
    internal sealed partial class CmdPalTranslatorSettingManager
    {
        private const string TranslateOperatorSettingId = "translateOperator";

        private TextSetting _translateOperatorSetting = null!;

        private void InitializeTranslateOperatorSettings()
        {
            _translateOperatorSetting = new TextSetting(TranslateOperatorSettingId, _translatorSettingService.TranslateOperator)
            {
                Label = "Translate operator",
                Description = "Use this operator in queries to override target language (for example: hello world -> ja).",
                Placeholder = TranslatorService.DefaultTranslateOperator,
            };

            _settings.Add(_translateOperatorSetting);
            ApplyTranslateOperatorSettingValue(_translatorSettingService.TranslateOperator);
            _settings.SettingsChanged += OnTranslateOperatorSettingsChanged;
        }

        private void OnTranslateOperatorSettingsChanged(object sender, Settings args)
        {
            string? translateOperator = _settings.GetSetting<string>(TranslateOperatorSettingId);
            if (translateOperator is not null)
            {
                _translatorSettingService.SetTranslateOperator(translateOperator);
            }
        }

        private void ApplyTranslateOperatorSettingValue(string translateOperator)
        {
            if (!string.Equals(_translateOperatorSetting.Value, translateOperator, StringComparison.Ordinal))
            {
                _translateOperatorSetting.Value = translateOperator;
            }
        }
    }
}
