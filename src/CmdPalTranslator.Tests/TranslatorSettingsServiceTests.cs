using CmdPalTranslator.Models;
using CmdPalTranslator.Services;

namespace CmdPalTranslator.Tests
{
    [TestClass]
    [TestCategory("Unit")]
    public sealed class TranslatorSettingsServiceTests
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

        /// <summary>當設定檔不存在時，應使用內建預設目標語言</summary>
        [TestMethod]
        public void UsesBuiltInDefaultTargetWhenSettingsFileDoesNotExist()
        {
            TranslatorSettingsService service = new(_settingsFilePath);

            Assert.AreEqual(LanguageCatalog.BuiltInDefaultTarget.Id, service.TargetLanguage.Id);
        }

        /// <summary>當設定檔包含有效的 JSON 內容時，應正確載入已儲存的語言設定</summary>
        [TestMethod]
        public void LoadsSavedLanguageFromValidJsonSettingsFile()
        {
            File.WriteAllText(_settingsFilePath, """{"targetLanguageId":"ja"}""");

            TranslatorSettingsService service = new(_settingsFilePath);

            Assert.AreEqual("ja", service.TargetLanguage.Id);
        }

        /// <summary>當設定檔包含無效的 JSON 內容時，應退回使用預設目標語言而非拋出例外</summary>
        [TestMethod]
        public void FallsBackToDefaultWhenSettingsFileContainsInvalidJson()
        {
            File.WriteAllText(_settingsFilePath, "not valid json");

            TranslatorSettingsService service = new(_settingsFilePath);

            Assert.AreEqual(LanguageCatalog.BuiltInDefaultTarget.Id, service.TargetLanguage.Id);
        }

        /// <summary>當設定檔中的 targetLanguageId 為空字串時，應退回使用預設目標語言</summary>
        [TestMethod]
        public void FallsBackToDefaultWhenTargetLanguageIdIsEmpty()
        {
            File.WriteAllText(_settingsFilePath, """{"targetLanguageId":""}""");

            TranslatorSettingsService service = new(_settingsFilePath);

            Assert.AreEqual(LanguageCatalog.BuiltInDefaultTarget.Id, service.TargetLanguage.Id);
        }

        /// <summary>當設定檔中的 targetLanguageId 為無法辨識的語言代碼時，應退回使用預設目標語言</summary>
        [TestMethod]
        public void FallsBackToDefaultWhenTargetLanguageIdIsUnknown()
        {
            File.WriteAllText(_settingsFilePath, """{"targetLanguageId":"unknown"}""");

            TranslatorSettingsService service = new(_settingsFilePath);

            Assert.AreEqual(LanguageCatalog.BuiltInDefaultTarget.Id, service.TargetLanguage.Id);
        }

        /// <summary>當設定檔中的 targetLanguageId 為 auto（自動偵測）時，因不可作為目標語言，應退回使用預設值</summary>
        [TestMethod]
        public void FallsBackToDefaultWhenTargetLanguageIdIsAutoDetect()
        {
            File.WriteAllText(_settingsFilePath, """{"targetLanguageId":"auto"}""");

            TranslatorSettingsService service = new(_settingsFilePath);

            Assert.AreEqual(LanguageCatalog.BuiltInDefaultTarget.Id, service.TargetLanguage.Id);
        }

        /// <summary>設定不同的目標語言時，應回傳 true 並更新 TargetLanguage 屬性</summary>
        [TestMethod]
        public void SetTargetLanguageReturnsTrueAndUpdatesPropertyForNewLanguage()
        {
            TranslatorSettingsService service = new(_settingsFilePath);

            bool result = service.SetTargetLanguage(LanguageCatalog.GetById("ja"));

            Assert.IsTrue(result);
            Assert.AreEqual("ja", service.TargetLanguage.Id);
        }

        /// <summary>設定與目前相同的目標語言時，應回傳 false 表示未變更</summary>
        [TestMethod]
        public void SetTargetLanguageReturnsFalseForSameLanguage()
        {
            TranslatorSettingsService service = new(_settingsFilePath);
            service.SetTargetLanguage(LanguageCatalog.GetById("ja"));

            bool result = service.SetTargetLanguage(LanguageCatalog.GetById("ja"));

            Assert.IsFalse(result);
        }

        /// <summary>傳入 null 時，應拋出 ArgumentNullException</summary>
        [TestMethod]
        public void SetTargetLanguageThrowsArgumentNullExceptionForNull()
        {
            TranslatorSettingsService service = new(_settingsFilePath);

            Assert.ThrowsExactly<ArgumentNullException>(() => service.SetTargetLanguage(null!));
        }

        /// <summary>成功變更目標語言時，應觸發 SettingsChanged 事件</summary>
        [TestMethod]
        public void SetTargetLanguageRaisesSettingsChangedEvent()
        {
            TranslatorSettingsService service = new(_settingsFilePath);
            bool eventRaised = false;
            service.SettingsChanged += (_, _) => eventRaised = true;

            service.SetTargetLanguage(LanguageCatalog.GetById("fr"));

            Assert.IsTrue(eventRaised);
        }

        /// <summary>設定與目前相同的目標語言時，不應觸發 SettingsChanged 事件</summary>
        [TestMethod]
        public void SetTargetLanguageDoesNotRaiseSettingsChangedEventForSameLanguage()
        {
            TranslatorSettingsService service = new(_settingsFilePath);
            service.SetTargetLanguage(LanguageCatalog.GetById("fr"));

            bool eventRaised = false;
            service.SettingsChanged += (_, _) => eventRaised = true;
            service.SetTargetLanguage(LanguageCatalog.GetById("fr"));

            Assert.IsFalse(eventRaised);
        }

        /// <summary>設定目標語言後，應將設定以 JSON 格式寫入檔案</summary>
        [TestMethod]
        public void SetTargetLanguagePersistsSettingAsJson()
        {
            TranslatorSettingsService service = new(_settingsFilePath);

            service.SetTargetLanguage(LanguageCatalog.GetById("ko"));

            Assert.IsTrue(File.Exists(_settingsFilePath));
            string json = File.ReadAllText(_settingsFilePath);
            Assert.IsTrue(json.Contains("\"targetLanguageId\""));
            Assert.IsTrue(json.Contains("\"ko\""));
        }

        /// <summary>已儲存的設定應能被新建立的實例正確讀取，驗證持久化的完整性</summary>
        [TestMethod]
        public void PersistedSettingCanBeReloadedByNewInstance()
        {
            TranslatorSettingsService service1 = new(_settingsFilePath);
            service1.SetTargetLanguage(LanguageCatalog.GetById("es"));

            TranslatorSettingsService service2 = new(_settingsFilePath);

            Assert.AreEqual("es", service2.TargetLanguage.Id);
        }

        /// <summary>傳入 AutoDetect 作為目標語言時，因會被正規化為預設值，應回傳 false 且維持預設語言</summary>
        [TestMethod]
        public void SetTargetLanguageFallsBackToDefaultForAutoDetect()
        {
            TranslatorSettingsService service = new(_settingsFilePath);

            bool result = service.SetTargetLanguage(LanguageCatalog.AutoDetect);

            Assert.IsFalse(result);
            Assert.AreEqual(LanguageCatalog.BuiltInDefaultTarget.Id, service.TargetLanguage.Id);
        }

        /// <summary>TargetLanguage 屬性應回傳包含正確 Id 與 DisplayName 的 LanguageOption 物件</summary>
        [TestMethod]
        public void TargetLanguageReturnsLanguageOptionWithCorrectDisplayName()
        {
            TranslatorSettingsService service = new(_settingsFilePath);
            service.SetTargetLanguage(LanguageCatalog.GetById("ja"));

            LanguageOption target = service.TargetLanguage;

            Assert.AreEqual("ja", target.Id);
            Assert.AreEqual("Japanese", target.DisplayName);
        }
    }
}
