namespace patter_pal.Util
{
    using System.Collections.Generic;

    public static class LanguageConstants
    {
        public const string DefaultLanguage = "en-US";

        public static readonly IDictionary<string, string> Languages = new Dictionary<string, string>
        {
            // Preview languages commented out
            {"ar-SA", "Arabic (Saudi Arabia)"},
            //{"zh-HK1", "Chinese (Cantonese, Traditional)"},
            {"zh-CN", "Chinese (Mandarin, Simplified)"},
            {"en-AU", "English (Australia)"},
            {"en-CA", "English (Canada)"},
            {"en-IN", "English (India)"},
            {"en-GB", "English (United Kingdom)"},
            {"en-US", "English (United States)"},
            {"fr-CA", "French (Canada)"},
            {"fr-FR", "French (France)"},
            {"de-DE", "German (Germany)"},
            //{"hi-IN1", "Hindi (India)"},
            {"it-IT", "Italian (Italy)"},
            {"ja-JP", "Japanese (Japan)"},
            {"ko-KR", "Korean (Korea)"},
            {"ms-MY", "Malay (Malaysia)"},
            {"nb-NO", "Norwegian Bokmål (Norway)"},
            {"pt-BR", "Portuguese (Brazil)"},
            //{"ru-RU1", "Russian (Russia)"},
            {"es-MX", "Spanish (Mexico)"},
            {"es-ES", "Spanish (Spain)"},
            //{"sv-SE1", "Swedish (Sweden)"},
            //{"ta-IN1", "Tamil (India)"},
            //{"vi-VN1", "Vietnamese (Vietnam)"}
        }; 
    }
}
