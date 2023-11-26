using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using patter_pal.Controllers;
using patter_pal.domain.Config;
using patter_pal.domain.Data;
using patter_pal.Logic.Data;
using patter_pal.Models;
using patter_pal.Util;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace patter_pal.Logic
{
    public class OpenAiService
    {
        private const int MIN_PROMPTS = 3; // Includes initial language prompt, feedback prompt and user input. Should always be at least those.
        private const int MIN_WORDS_USER_INPUT = 14; // Will only reduce the user input if it is longer than this
        private static readonly char[] WORD_SEPERATORS = { ' ', '\r', '\n' };

        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppConfig _appConfig;

        public OpenAiService(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, AppConfig appConfig)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _appConfig = appConfig;
        }

        public async Task<string?> StreamAndGenerateAnswer(WebSocket ws, SpeechRecognitionResult reconitionResult, ConversationData conversation, string language)
        {
            _logger.LogDebug($"Generating Answer from WebSocket with language {language} {conversation.Id}");
            // Provide the language prompt, pronounciation assessment and the user input
            string languagePrompt = PromptForLanguage(_appConfig.OpenAiSystemHelperPrompt, language);
            var messages = new List<OpenAiMessage>() {
                new OpenAiMessage { Role = OpenAiMessage.ROLE_SYSTEM, Content = languagePrompt },
                new OpenAiMessage { Role = OpenAiMessage.ROLE_SYSTEM, Content = ExtractPronounciationAssesmentString(reconitionResult) },
                new OpenAiMessage { Role = OpenAiMessage.ROLE_USER, Content = reconitionResult.Text } 
            };

            // Add previous message history from conversationId when present
            if (conversation.Data.Any())
            {
                messages.InsertRange(0, conversation.Data.Select(
                    (chat) => new OpenAiMessage { Role = chat.IsUser ? OpenAiMessage.ROLE_USER : OpenAiMessage.ROLE_ASSISTANT, Content = chat.Text })
                    );
            }

            // Reduce messages to token limit if possible
            try { ReduceToTokenLimit(messages); }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Could not reduce messages to token limit, aborting");
                await WebSocketHelper.SendTextWhenOpen(ws, JsonSerializer.Serialize(new SocketResult<ErrorResponse>(new ErrorResponse("Please open a new conversation. This conversation has reached its limit.", ErrorResponse.ErrorCode.OpenAiServiceError), SocketResultType.Error)));
                return null;
            }

            // Performing the actual request
            string? result = await InnerStreamResult(ws, language, conversation, messages);
            return result;
        }

        /// <summary>
        /// Performs the HTTP request and streams the result to the client
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="language"></param>
        /// <param name="conversation"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        private async Task<string?> InnerStreamResult(WebSocket ws, string language, ConversationData conversation, List<OpenAiMessage> messages)
        {
            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _appConfig.OpenAiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var chatRequest = new OpenAiChatRequest(_appConfig, messages);

            string? result = null;
            try
            {
                _logger.LogDebug($"Sending to OpenAI: {string.Join("\n", messages)}");
                var requestContent = JsonContent.Create(chatRequest);
                var request = new HttpRequestMessage(HttpMethod.Post, _appConfig.OpenAiEndpoint)
                {
                    Content = requestContent
                };

                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                var answerContentBuilder = new StringBuilder();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);

                await BuildAnswerFromStream(ws, answerContentBuilder, reader);

                result = answerContentBuilder.ToString();
                _logger.LogDebug($"Got full answer from OpenAI: {result}");
            }
            catch (HttpRequestException e)
            {
                ErrorResponse? error = null;
                _logger.LogWarning(e, $"HttpRequestException while sending to OpenAI: {e.StatusCode}: {e.Message}");
                var statusCode = (int?)e.StatusCode;

                // Check if status is 4xx then our ratelimit it at stake
                if (statusCode != null && statusCode >= 400 && statusCode < 500)
                {
                    _logger.LogError("Got 4XX response from OpenAi, a limit might be reached.");
                    error = new ErrorResponse("Response limit reached. Please try again later.", ErrorResponse.ErrorCode.OpenAiServiceError);
                }
                else error = new ErrorResponse("OpenAI is currently unavailable. Please try again later.", ErrorResponse.ErrorCode.OpenAiServiceError);

                await WebSocketHelper.SendTextWhenOpen(ws, JsonSerializer.Serialize(new SocketResult<ErrorResponse>(error, SocketResultType.Error)));
            }
            catch (Exception e)
            {
                // Most likely timeout
                _logger.LogError(e, "Other unhandeled error while sending to OpenAI");
                var error = new ErrorResponse("OpenAI is taking to long to respond. Please try again later.", ErrorResponse.ErrorCode.OpenAiServiceError);
                await WebSocketHelper.SendTextWhenOpen(ws, JsonSerializer.Serialize(new SocketResult<ErrorResponse>(error, SocketResultType.Error)));
            }

            return result;
        }

        private async Task BuildAnswerFromStream(WebSocket ws, StringBuilder responseContentBuilder, StreamReader reader)
        {
            var sendTasks = new List<Task>();

            while (!reader.EndOfStream && ws.State == WebSocketState.Open)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.Equals(OpenAiChatCompletionResponse.DONE_RESPONSE) == true) break;

                try
                {
                    // Remove "data: '" from the start and "'" from the end in one statement, really weird of the OpenAi devs here
                    string json = line[6..].Trim();
                    var chunk = JsonSerializer.Deserialize<OpenAiChatCompletionResponse>(json);
                    if (chunk == null || chunk.Content == null) throw new ApplicationException("Could not get content from OpenAi response.");

                    var content = chunk.Content;
                    sendTasks.Add(WebSocketHelper.SendTextWhenOpen(ws, JsonSerializer.Serialize(new SocketResult<string>(content, SocketResultType.PartialAnswer)))); // Pass intermediate results to client
                    responseContentBuilder.Append(content);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error in deserializing OpenAi response stream.");
                }
            }

            await Task.WhenAll(sendTasks);
        }

        /// <summary>
        /// "Forgets" older messages when token limit is exceeded.
        /// Reduces the message input if still to large.
        /// This is one possible approach, others would include summing up the previous content, or using embeddings.
        /// </summary>
        /// <param name="messages"></param>
        /// <exception cref="Exception"></exception>
        private void ReduceToTokenLimit(List<OpenAiMessage> messages)
        {
            while (TokenHelper.ExceedsTokenLimit(messages.Select(m => m.Content), _appConfig.OpenAiMaxInputTokens))
            {
                if (messages.Count > MIN_PROMPTS)
                {
                    // Remove oldest message
                    _logger.LogDebug($"Reducing messages to token limit, removing oldest message: {messages[0].Content}");
                    messages.RemoveAt(0);
                }
                else
                {
                    var lastMessage = messages.Last().Content;
                    var words = lastMessage.Split(WORD_SEPERATORS, StringSplitOptions.RemoveEmptyEntries);

                    if (words.Length > MIN_WORDS_USER_INPUT)
                    {
                        // Remove last word
                        _logger.LogDebug($"Reducing messages to token limit, removing last word: {words[^1]}");
                        messages.Last().Content = string.Join(" ", words.Take(words.Length - 1));
                    }
                    else if (messages.Count == 3)
                    {
                        // Remove feedback metrics prompt
                        messages.RemoveAt(1);
                    }
                    else
                    {
                        throw new Exception("Cannot reduce messages any further");
                    }
                }

            }
        }

        /// <summary>
        /// Fills the prompt with the specified language in the format ex. German (Austria)
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        private string PromptForLanguage(string prompt, string lang)
        {
            LanguageConstants.Languages.TryGetValue(lang, out string? langDescriptor);
            langDescriptor ??= lang; // Fallback to language code if no descriptor is found

            string languageName = langDescriptor;
            string countryName = langDescriptor;

            // Extracting the main language name and country name (inside parentheses if any)
            int parenthesisIndex = langDescriptor.IndexOf(" (");
            if (parenthesisIndex > 0)
            {
                languageName = langDescriptor.Substring(0, parenthesisIndex);
                int endParenthesisIndex = langDescriptor.IndexOf(')', parenthesisIndex);
                if (endParenthesisIndex > parenthesisIndex)
                {
                    countryName = langDescriptor.Substring(parenthesisIndex + 2, endParenthesisIndex - parenthesisIndex - 2);
                }
            }

            // Format the prompt with the specified language
            return string.Format(prompt, countryName, languageName);
        }

        /// <summary>
        /// Extracts a string that can be used as prompt input including the pronounciation assessment.
        /// </summary>
        /// <param name="reconitionResult"></param>
        /// <returns></returns>
        private string ExtractPronounciationAssesmentString(SpeechRecognitionResult reconitionResult)
        {
            var pronounciationResult = PronunciationAssessmentResult.FromResult(reconitionResult);
            var feedback = new StringBuilder("Metrics about user speech:{");

            // Adding overall pronunciation metrics
            feedback.Append($"Accuracy:{pronounciationResult.AccuracyScore};");
            feedback.Append($"Fluency:{pronounciationResult.FluencyScore};");
            feedback.Append($"Pronunciation:{pronounciationResult.PronunciationScore};");
            if (pronounciationResult.ProsodyScore != 0) feedback.Append($"Prosody:{pronounciationResult.ProsodyScore};");

            // Filtering and adding words with errors
            var errorWordsFeedback = pronounciationResult.Words
                .Where(word => word.ErrorType != "None")
                .Select(word => $"{word.Word}={word.ErrorType}");

            if (errorWordsFeedback.Any())
            {
                feedback.Append("Word-Errors:");
                feedback.Append(string.Join(",", errorWordsFeedback));
                feedback.Append(';');
            }

            feedback.Append('}');
            return feedback.ToString();
        }
    }
}
