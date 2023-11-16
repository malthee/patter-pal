using System.Diagnostics;

namespace patter_pal.Util
{
    public class AppConfig
    {
        public const string AppUrl = "https://patter-pal.azurewebsites.net";
        public const string OpenAIEndpoint = "https://api.openai.com/v1/chat/completions";
        public const string SpeechWsEndpoint = "/Speech/RecognizeWs";
        public const int TargetSampleRate = 16000; // Required by Speech SDK

        // -- If one of the following values is changed, the other values must be changed accordingly --
        public const int RecordingChunkTimeMs = 3000; // How often the audio stream is sent to the server
        public const int RecordingBufferSize = 4 * 4096; 
        public const int SpeechWsBuffer = 1024 * 100; 
        // -- End --

        public string SpeechSubscriptionKey { get; set; } = default!;
        public string SpeechRegion { get; set; } = default!;
        public string SpeechVoice { get; set; } = default!;
        public string OpenAiKey { get; set; } = default!;

        public string OpenAiModel { get; set; } = "gpt-3.5-turbo";
        public double OpenAiTemperature { get; set; } = 1.4;
        public int OpenAiMaxTokens { get; set; } = 256;
        public double OpenAiTopP { get; set; } = 1;
        public double OpenAiFrequencyPenalty { get; set; } = 0.2;
        public double OpenAiPresencePenalty { get; set; } = 0.2;
    }
}
