namespace patter_pal.domain.Config
{
    public class AppConfig
    {
        // -- Compile time app constants -- //
        public const string AppUrl = "https://patter-pal.azurewebsites.net";
        public const string ConversationWebSocket = "WebSocket/StartConversation";
        public const string ConversationRestEndpoint = "Conversation";
        public const string ChatRestEndpoint = "Conversation/Chat";
        public const string StatsEndpoint = "Stats";
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
        public string CosmosDbName { get; set; } = "db1";
        public string CosmosDbConversationContainer { get; set; } = "c1";
        public string CosmosDbConversationPk { get; set; } = "/UserId";
        public string CosmosDbPronounciationResultContainer { get; set; } = "c2";
        public string CosmodDbPronounciationResultPk { get; set; } = "/UserId";
        /// <summary>
        /// Seperated by ; these are 10-char codes in the fommat of abcde-abcde that allow "special login access"  
        /// </summary>
        public string ValidSpecialCodes { get; set; } = "while-devin;remov-iprod";
        public int PronounciationAnalyticsMaxWordCount { get; set; } = 10;

        // --- Transfer settings --- //
        public int HttpTimeout { get; set; } = 20; // Seconds
        public int WebSocketKeepAlive { get; set; } = 1; // Minutes

        // --- OpenAI API --- //
        public string OpenAiEndpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
        public string OpenAiModel { get; set; } = "gpt-3.5-turbo";
        public double OpenAiTemperature { get; set; } = 1.1;
        public int OpenAiMaxOutputTokens { get; set; } = 256; // For output
        public int OpenAiMaxInputTokens { get; set; } = 1024; // gpt 3.5 turbo is 4096, reduced to save computing ressources
        public double OpenAiTopP { get; set; } = 1;
        public double OpenAiFrequencyPenalty { get; set; } = 0.2;
        public double OpenAiPresencePenalty { get; set; } = 0.2;
        // TODO improve, may also tune parameters above?
        public string OpenAiSystemHelperPrompt { get; set; } = @"As a 30-year-old language teacher from {0}, with expertise in {1}, engage in a student-teacher dialogue. 
Remember, you're a real person, not an AI. Respond within 50 words, correcting their language use. 
Briefly comment on their pronunciation aspects (accuracy, fluency, prosody, mispronunciations) without using specific metrics. 
Provide clear, constructive feedback to improve their language skills. Aim for concise, non-repetitive responses.";

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
