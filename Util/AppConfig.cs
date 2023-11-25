using System.Diagnostics;

namespace patter_pal.Util
{
    public class AppConfig
    {
        // -- Compile time app constants -- //
        public const string AppUrl = "https://patter-pal.azurewebsites.net";
        public const string ConversationWebSocket = "WebSocket/StartConversation";
        // These properties are not required to be set in the environment variables
        public readonly string[] NonRequiredProperties = { nameof(ValidSpecialCodes) };

        // --- Audio Recording: If one of the following values is changed, the other values must be changed accordingly --- //
        public const int TargetSampleRate = 16000; // Required by Speech SDK
        public const int RecordingChunkTimeMs = 3000; // How often the audio stream is sent to the server
        public const int RecordingBufferSize = 4 * 4096;
        public const int SpeechWsBuffer = 1024 * 100;

        // -- Environment variables -- //
        public string SpeechSpeakerVoice { get; set; } = "en-US-JennyMultilingualV2Neural"; // Has to support all languages from LanguageConstants
        public string SpeechSubscriptionKey { get; set; } = string.Empty;
        public string SpeechRegion { get; set; } = string.Empty;
        public string OpenAiKey { get; set; } = string.Empty;
        public string GoogleOAuthClientID { get; set; } = string.Empty;
        public string GoogleOAuthClientSecret { get; set; } = string.Empty;
        public string DbConnectionString { get; set; } = string.Empty;
        public string CosmosDbDb1 { get; set; } = "db1";
        public string CosmosDbDb1C1 { get; set; } = "c1";
        public string CosmosDbDb1C1PK { get; set; } = "/UserId";
        public string CosmosDbDb1C2 { get; set; } = "c2";
        public string CosmosDbDb1C2PK { get; set; } = "/UserId";
        // Seperated by ; these are 10-char codes in the fommat of abcde-abcde that allow "special login access"  
        public string ValidSpecialCodes { get; set; } = string.Empty;

        // --- Transfer settings --- //
        public int HttpTimeout { get; set; } = 20; // Seconds
        public int WebSocketKeepAlive { get; set; } = 1; // Minutes
        // Seconds how long server waits for audio before closing, should be dependent on RecordingChunkTime as user only sends every x seconds
        //public int AudioRecognitionTimeoutServer { get; set; } = RecordingChunkTimeMs / 1000 + 3; 

        // --- OpenAI API --- //
        public string OpenAiEndpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
        public string OpenAiModel { get; set; } = "gpt-3.5-turbo";
        public double OpenAiTemperature { get; set; } = 1.1;
        public int OpenAiMaxOutputTokens { get; set; } = 256; // For output
        public int OpenAiMaxInputTokens { get; set; } = 1024; // gpt 3.5 turbo is 4096, reduced to save computing ressources
        public double OpenAiTopP { get; set; } = 1;
        public double OpenAiFrequencyPenalty { get; set; } = 0.2;
        public double OpenAiPresencePenalty { get; set; } = 0.2;
        public string OpenAiSystemHelperPrompt { get; set; } = @"As a 30-year-old language teacher native to {0}, you specialize in teaching {1}. 
Engage in a supportive, friendly dialogue with your student. 
Respond concisely (max 50 words) while correcting any language errors in their message. Double check if they made any errors.
Utilize provided metrics in pronunciation assessment (accuracy, fluency, prosody, and mispronunciations). 
Give clear and constructive feedback to help enhance their language proficiency. But keep it concise.";

        public void ValidateConfigInitialized()
        {
            // Any of the values is not set, check with reflection
            foreach (var property in typeof(AppConfig).GetProperties())
            {
                if (!NonRequiredProperties.Contains(property.Name) && string.IsNullOrEmpty(property.GetValue(this)?.ToString()))
                {
                    throw new ArgumentException($"AppConfig property {property.Name} is not set");
                }
            }
        }
    }
}
