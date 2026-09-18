const translations = {
  zh: {
    navFeatures: "功能",
    navHow: "怎麼用",
    navFaq: "常見問題",
    heroTitle: "翻譯，<em>就在手邊。</em>",
    heroLede: "Translator for Command Palette 把文字翻譯放進 Windows Command Palette。叫出 Translator、輸入文字，結果就會回到你正在工作的地方。",
    installStore: "從 Microsoft Store 安裝 <span aria-hidden=\"true\">↗</span>",
    viewSource: "查看 GitHub 原始碼 <span aria-hidden=\"true\">↗</span>",
    heroNote: "適用於 Windows 11 · MIT License · 由 Poy Chang 發佈",
    heroCaption: "實際畫面：輸入內容後，翻譯結果與語言配對會直接顯示。",
    proofOne: "一個命令",
    proofOneSub: "叫出 Translator",
    proofThree: "三個來源",
    proofThreeSub: "Bing · Google · Aliyun",
    proofSyntax: "一套語法",
    proofSyntaxSub: "text >> languageCode",
    featuresTitle: "少一個切換視窗，<br><em>多一點專注。</em>",
    featureOneTitle: "Command Palette 原生流程",
    featureOneText: "搜尋並執行 <code>Translator</code>，不必另開瀏覽器。輸入框、結果與更多操作都在同一個工作流。",
    featureTwoTitle: "目標語言，隨手指定",
    featureTwoText: "使用預設的 <code>>></code> 語法覆寫單次翻譯目標，也能在 Target language 頁面保存常用語言。",
    featureThreeTitle: "結果不只看，還能帶走",
    featureThreeText: "翻譯結果可直接複製；更多操作可複製原文，或開啟對應的翻譯服務網頁。",
    demoTitle: "三步，完成一次翻譯。",
    demoIntro: "以下為固定的操作示意，不會呼叫真實翻譯 API。實際結果會依你選擇的 provider 與網路狀況而定。",
    stepOneTitle: "開啟 Translator",
    stepOneText: "在 Windows Command Palette 搜尋並執行 <code>Translator</code>。",
    stepTwoTitle: "輸入要翻譯的文字",
    stepTwoText: "直接輸入，例如 <code>hello world</code>。若未指定語言，會使用已設定的目標語言。",
    stepThreeTitle: "指定目標語言，或直接複製",
    stepThreeText: "輸入 <code>hello world >> ja</code> 指定日文；在結果上執行 Copy 即可帶走翻譯。",
    galleryTitle: "熟悉的工具感，<br><em>清楚的結果。</em>",
    galleryIntro: "以下皆為儲存庫中的實際產品畫面。",
    galleryOne: "輸入提示與目標語言",
    galleryTwo: "選擇目標語言",
    installTitle: "把下一次翻譯，<br>留在你的工作流裡。",
    installText: "一般使用者只需要 Windows 11、網路連線，以及從 Microsoft Store 安裝。開發者則可依專案文件使用 .NET 10 與 Windows 建置工具。",
    openStore: "前往 Microsoft Store <span aria-hidden=\"true\">↗</span>",
    faqTitle: "常見問題",
    faqOneTitle: "需要網路連線嗎？",
    faqOneText: "需要。翻譯請求會透過網路送至所選的第三方翻譯服務；應用程式具備 <code>internetClient</code> 能力。",
    faqTwoTitle: "翻譯結果來自哪裡？",
    faqTwoText: "目前程式內建 Bing、Google 與 Aliyun 三個 provider，預設使用 Bing，可在 Preferred provider 設定中選擇。",
    faqThreeTitle: "可以設定預設語言嗎？",
    faqThreeText: "可以。在翻譯頁面的 Target language 開啟語言設定並選取語言。內建語言包含 English、繁體中文、簡體中文、日文、韓文、法文、德文等。",
    faqFourTitle: "我的文字會被儲存嗎？",
    faqFourText: "依隱私權文件，應用程式與開發者不保留文字輸入或翻譯結果；翻譯時，文字與語言代碼會直接傳送給你選擇的第三方服務。",
    footerText: "Windows Command Palette 的翻譯擴充功能。",
    reportIssue: "問題回報",
    privacy: "隱私權"
  },
  en: {
    navFeatures: "Features",
    navHow: "How it works",
    navFaq: "FAQ",
    heroTitle: "Translate, <em>right at hand.</em>",
    heroLede: "Translator for Command Palette brings text translation into Windows Command Palette. Open Translator, enter your text, and get the result without leaving your work.",
    installStore: "Install from Microsoft Store <span aria-hidden=\"true\">↗</span>",
    viewSource: "View source on GitHub <span aria-hidden=\"true\">↗</span>",
    heroNote: "For Windows 11 · MIT License · Published by Poy Chang",
    heroCaption: "Actual product screen: translation results and language pairs appear as you type.",
    proofOne: "One command",
    proofOneSub: "Open Translator",
    proofThree: "Three providers",
    proofThreeSub: "Bing · Google · Aliyun",
    proofSyntax: "One syntax",
    proofSyntaxSub: "text >> languageCode",
    featuresTitle: "One less window,<br><em>more focus.</em>",
    featureOneTitle: "A native Command Palette flow",
    featureOneText: "Search for and run <code>Translator</code> without opening another browser window. Input, results, and actions stay together.",
    featureTwoTitle: "Choose a target on demand",
    featureTwoText: "Use the default <code>>></code> syntax to override the target for one translation, or save a preferred language.",
    featureThreeTitle: "Results you can take with you",
    featureThreeText: "Copy a translation directly, copy the source text, or open the matching provider website from more actions.",
    demoTitle: "Three steps to a translation.",
    demoIntro: "This is a fixed interaction example and does not call a live translation API. Actual results depend on your provider and network.",
    stepOneTitle: "Open Translator",
    stepOneText: "Search for and run <code>Translator</code> in Windows Command Palette.",
    stepTwoTitle: "Enter text to translate",
    stepTwoText: "Type something such as <code>hello world</code>. Without a target, the configured language is used.",
    stepThreeTitle: "Set a target, or copy the result",
    stepThreeText: "Enter <code>hello world >> ja</code> for Japanese, then use Copy on the result to take it with you.",
    galleryTitle: "A familiar tool,<br><em>clear results.</em>",
    galleryIntro: "These are actual product screens from the repository.",
    galleryOne: "Input prompt and target language",
    galleryTwo: "Choose a target language",
    installTitle: "Keep your next translation<br>in your workflow.",
    installText: "For everyday use, you need Windows 11, an internet connection, and the Microsoft Store installation. Developers can use .NET 10 and Windows build tools as documented in the project.",
    openStore: "Open Microsoft Store <span aria-hidden=\"true\">↗</span>",
    faqTitle: "Frequently asked questions",
    faqOneTitle: "Does it need an internet connection?",
    faqOneText: "Yes. Translation requests are sent to the selected third-party provider over the internet, using the app's <code>internetClient</code> capability.",
    faqTwoTitle: "Where do translations come from?",
    faqTwoText: "The app currently includes Bing, Google, and Aliyun providers. Bing is the default and can be changed in Preferred provider settings.",
    faqThreeTitle: "Can I set a default language?",
    faqThreeText: "Yes. Open Target language from the translation page and choose a language. The built-in catalog includes English, Traditional Chinese, Simplified Chinese, Japanese, Korean, French, German, and more.",
    faqFourTitle: "Is my text stored?",
    faqFourText: "According to the privacy policy, the app and developer do not retain input or translation results. Text and language codes are sent directly to the third-party service you choose.",
    footerText: "Translation for Windows Command Palette.",
    reportIssue: "Report an issue",
    privacy: "Privacy"
  }
};

const bindings = {
  "#hero-title": "heroTitle", ".hero-lede": "heroLede", ".hero-actions .button-primary": "installStore",
  ".hero-actions .button-quiet": "viewSource", ".hero-note": "heroNote", ".hero-visual figcaption": "heroCaption",
  ".proof-strip div:nth-child(1) strong": "proofOne", ".proof-strip div:nth-child(1) span": "proofOneSub",
  ".proof-strip div:nth-child(2) strong": "proofThree", ".proof-strip div:nth-child(2) span": "proofThreeSub",
  ".proof-strip div:nth-child(3) strong": "proofSyntax", ".proof-strip div:nth-child(3) span": "proofSyntaxSub",
  "#features .feature-card:nth-child(1) h3": "featureOneTitle", "#features .feature-card:nth-child(1) p": "featureOneText",
  "#features .feature-card:nth-child(2) h3": "featureTwoTitle", "#features .feature-card:nth-child(2) p": "featureTwoText",
  "#features .feature-card:nth-child(3) h3": "featureThreeTitle", "#features .feature-card:nth-child(3) p": "featureThreeText",
  "#demo-title": "demoTitle", ".demo-layout .section-heading > p:last-child": "demoIntro",
  ".step:nth-child(1) h3": "stepOneTitle", ".step:nth-child(1) p": "stepOneText", ".step:nth-child(2) h3": "stepTwoTitle",
  ".step:nth-child(2) p": "stepTwoText", ".step:nth-child(3) h3": "stepThreeTitle", ".step:nth-child(3) p": "stepThreeText",
  "#gallery-title": "galleryTitle", ".inline-heading > p": "galleryIntro", ".screenshot-grid figure:nth-child(1) figcaption": "galleryOne",
  ".screenshot-grid figure:nth-child(2) figcaption": "galleryTwo", "#install-title": "installTitle", ".install-layout > div:nth-child(2) p": "installText",
  ".install-layout .button": "openStore", "#faq-title": "faqTitle", ".faq-list details:nth-child(1) summary": "faqOneTitle",
  ".faq-list details:nth-child(1) p": "faqOneText", ".faq-list details:nth-child(2) summary": "faqTwoTitle", ".faq-list details:nth-child(2) p": "faqTwoText",
  ".faq-list details:nth-child(3) summary": "faqThreeTitle", ".faq-list details:nth-child(3) p": "faqThreeText", ".faq-list details:nth-child(4) summary": "faqFourTitle",
  ".faq-list details:nth-child(4) p": "faqFourText", ".footer-inner p": "footerText", ".footer-inner nav a:nth-child(2)": "reportIssue", ".footer-inner nav a:nth-child(4)": "privacy"
};

function setLanguage(language) {
  const strings = translations[language];
  Object.entries(bindings).forEach(([selector, key]) => {
    const element = document.querySelector(selector);
    if (element) element.innerHTML = strings[key];
  });
  document.querySelectorAll("[data-i18n]").forEach(element => {
    element.textContent = strings[element.dataset.i18n];
  });
  document.documentElement.lang = language === "zh" ? "zh-Hant-TW" : "en";
  document.title = language === "zh"
    ? "Translator for Command Palette | 快速翻譯，不離開工作流"
    : "Translator for Command Palette | Translate without leaving your workflow";
  document.querySelector("meta[name=description]").content = language === "zh"
    ? "Translator for Command Palette：在 Windows Command Palette 中快速翻譯文字、指定目標語言並複製結果。"
    : "Translator for Command Palette brings quick text translation, target language selection, and copy actions to Windows Command Palette.";
  const switcher = document.querySelector("[data-language-switch]");
  switcher.textContent = language === "zh" ? "EN" : "繁中";
  switcher.setAttribute("aria-label", language === "zh" ? "Switch to English" : "切換至繁體中文");
  localStorage.setItem("cmdpal-language", language);
}

document.addEventListener("DOMContentLoaded", () => {
  const switcher = document.querySelector("[data-language-switch]");
  const savedLanguage = localStorage.getItem("cmdpal-language");
  if (savedLanguage === "zh") setLanguage("zh");
  switcher.addEventListener("click", () => setLanguage(document.documentElement.lang === "en" ? "zh" : "en"));
});