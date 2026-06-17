using CmdPalTranslator.Core.Models;
using CmdPalTranslator.Core.Services;

namespace CmdPalTranslator.Tests
{
    [TestClass]
    [TestCategory("Unit")]
    public sealed class CmdPalTranslatorSettingServiceTests
    {
        private string _settingsFilePath = null!;

        [TestInitialize]
        public void Setup()
        {
            _settingsFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_settingsFilePath))
            {
                File.Delete(_settingsFilePath);
            }
        }

        [TestMethod]
        public void UsesBuiltInDefaultTargetWhenSettingsFileDoesNotExist()
        {
            CmdPalTranslatorSettingService service = new(_settingsFilePath);

            Assert.AreEqual(LanguageCatalog.BuiltInDefaultTarget.Id, service.TargetLanguage.Id);
        }

        [TestMethod]
        public void UsesDefaultTranslateOperatorWhenSettingsFileDoesNotExist()
        {
            CmdPalTranslatorSettingService service = new(_settingsFilePath);

            Assert.AreEqual(TranslatorService.DefaultTranslateOperator, service.TranslateOperator);
        }

        [TestMethod]
        public void LoadsSavedLanguageFromValidJsonSettingsFile()
        {
            File.WriteAllText(_settingsFilePath, """{"targetLanguageId":"ja"}""");

            CmdPalTranslatorSettingService service = new(_settingsFilePath);

            Assert.AreEqual("ja", service.TargetLanguage.Id);
        }

        [TestMethod]
        public void LoadsSavedTranslateOperatorFromValidJsonSettingsFile()
        {
            File.WriteAllText(_settingsFilePath, """{"translateOperator":"=>"}""");

            CmdPalTranslatorSettingService service = new(_settingsFilePath);

            Assert.AreEqual("=>", service.TranslateOperator);
        }

        [TestMethod]
        public void FallsBackToDefaultWhenSettingsFileContainsInvalidJson()
        {
            File.WriteAllText(_settingsFilePath, "not valid json");

            CmdPalTranslatorSettingService service = new(_settingsFilePath);

            Assert.AreEqual(LanguageCatalog.BuiltInDefaultTarget.Id, service.TargetLanguage.Id);
        }

        [TestMethod]
        public void FallsBackToDefaultWhenTargetLanguageIdIsEmpty()
        {
            File.WriteAllText(_settingsFilePath, """{"targetLanguageId":""}""");

            CmdPalTranslatorSettingService service = new(_settingsFilePath);

            Assert.AreEqual(LanguageCatalog.BuiltInDefaultTarget.Id, service.TargetLanguage.Id);
        }

        [TestMethod]
        public void FallsBackToDefaultWhenTargetLanguageIdIsUnknown()
        {
            File.WriteAllText(_settingsFilePath, """{"targetLanguageId":"unknown"}""");

            CmdPalTranslatorSettingService service = new(_settingsFilePath);

            Assert.AreEqual(LanguageCatalog.BuiltInDefaultTarget.Id, service.TargetLanguage.Id);
        }

        [TestMethod]
        public void FallsBackToDefaultWhenTargetLanguageIdIsAutoDetect()
        {
            File.WriteAllText(_settingsFilePath, """{"targetLanguageId":"auto"}""");

            CmdPalTranslatorSettingService service = new(_settingsFilePath);

            Assert.AreEqual(LanguageCatalog.BuiltInDefaultTarget.Id, service.TargetLanguage.Id);
        }

        [TestMethod]
        public void SetTargetLanguageReturnsTrueAndUpdatesPropertyForNewLanguage()
        {
            CmdPalTranslatorSettingService service = new(_settingsFilePath);

            bool result = service.SetTargetLanguage(LanguageCatalog.GetById("ja"));

            Assert.IsTrue(result);
            Assert.AreEqual("ja", service.TargetLanguage.Id);
        }

        [TestMethod]
        public void SetTargetLanguageReturnsFalseForSameLanguage()
        {
            CmdPalTranslatorSettingService service = new(_settingsFilePath);
            service.SetTargetLanguage(LanguageCatalog.GetById("ja"));

            bool result = service.SetTargetLanguage(LanguageCatalog.GetById("ja"));

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SetTargetLanguageThrowsArgumentNullExceptionForNull()
        {
            CmdPalTranslatorSettingService service = new(_settingsFilePath);

            Assert.ThrowsExactly<ArgumentNullException>(() => service.SetTargetLanguage(null!));
        }

        [TestMethod]
        public void SetTargetLanguageRaisesSettingsChangedEvent()
        {
            CmdPalTranslatorSettingService service = new(_settingsFilePath);
            bool eventRaised = false;
            service.SettingsChanged += (_, _) => eventRaised = true;

            service.SetTargetLanguage(LanguageCatalog.GetById("fr"));

            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void SetTargetLanguageDoesNotRaiseSettingsChangedEventForSameLanguage()
        {
            CmdPalTranslatorSettingService service = new(_settingsFilePath);
            service.SetTargetLanguage(LanguageCatalog.GetById("fr"));

            bool eventRaised = false;
            service.SettingsChanged += (_, _) => eventRaised = true;
            service.SetTargetLanguage(LanguageCatalog.GetById("fr"));

            Assert.IsFalse(eventRaised);
        }

        [TestMethod]
        public void SetTargetLanguagePersistsSettingAsJson()
        {
            CmdPalTranslatorSettingService service = new(_settingsFilePath);

            service.SetTargetLanguage(LanguageCatalog.GetById("ko"));

            Assert.IsTrue(File.Exists(_settingsFilePath));
            string json = File.ReadAllText(_settingsFilePath);
            Assert.IsTrue(json.Contains("\"targetLanguageId\""));
            Assert.IsTrue(json.Contains("\"ko\""));
        }

        [TestMethod]
        public void PersistedSettingCanBeReloadedByNewInstance()
        {
            CmdPalTranslatorSettingService service1 = new(_settingsFilePath);
            service1.SetTargetLanguage(LanguageCatalog.GetById("es"));

            CmdPalTranslatorSettingService service2 = new(_settingsFilePath);

            Assert.AreEqual("es", service2.TargetLanguage.Id);
        }

        [TestMethod]
        public void SetTranslateOperatorPersistsSettingAsJson()
        {
            CmdPalTranslatorSettingService service = new(_settingsFilePath);

            service.SetTranslateOperator("=>");

            Assert.AreEqual("=>", service.TranslateOperator);
            Assert.IsTrue(File.Exists(_settingsFilePath));
            string json = File.ReadAllText(_settingsFilePath);
            Assert.IsTrue(json.Contains("\"translateOperator\""));

            CmdPalTranslatorSettingService reloadedService = new(_settingsFilePath);
            Assert.AreEqual("=>", reloadedService.TranslateOperator);
        }

        [TestMethod]
        public void SetTargetLanguageFallsBackToDefaultForAutoDetect()
        {
            CmdPalTranslatorSettingService service = new(_settingsFilePath);

            bool result = service.SetTargetLanguage(LanguageCatalog.AutoDetect);

            Assert.IsFalse(result);
            Assert.AreEqual(LanguageCatalog.BuiltInDefaultTarget.Id, service.TargetLanguage.Id);
        }

        [TestMethod]
        public void TargetLanguageReturnsLanguageOptionWithCorrectDisplayName()
        {
            CmdPalTranslatorSettingService service = new(_settingsFilePath);
            service.SetTargetLanguage(LanguageCatalog.GetById("ja"));

            LanguageOption target = service.TargetLanguage;

            Assert.AreEqual("ja", target.Id);
            Assert.AreEqual("Japanese", target.DisplayName);
        }
    }
}
