using System.Diagnostics;

namespace patter_pal.Util
{
    public class AppConfig
    {
        public const string AppUrl = "https://patter-pal.azurewebsites.net";
        public const string SpeechWsEndpoint = "/Speech/Ws";
        public const int SpeechRecordTimesliceMs = 1000;
        public const int SpeechWsBuffer = 1024 * 1000;

        public string SpeechSubscriptionKey { get; set; } = default!;
        public string SpeechRegion { get; set; } = default!;
        public string SpeechVoice { get; set; } = default!;
    }
}
