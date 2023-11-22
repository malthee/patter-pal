using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using Microsoft.CognitiveServices.Speech;
using patter_pal.Util;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using patter_pal.Models;
using static patter_pal.Models.SpeechPronounciationResult;
using static System.Net.Mime.MediaTypeNames;

namespace patter_pal.Logic
{
    public class SpeechPronounciationService
    {
        private readonly ILogger<SpeechPronounciationService> _logger;
        private readonly AppConfig _appConfig;
        private readonly SpeechConfig _speechConfig;
        private readonly PronunciationAssessmentConfig _pronunciationAssessmentConfig;

        public SpeechPronounciationService(ILogger<SpeechPronounciationService> logger, AppConfig appConfig)
        {
            _logger = logger;
            _appConfig = appConfig;
            _speechConfig = SpeechConfig.FromSubscription(_appConfig.SpeechSubscriptionKey, _appConfig.SpeechRegion);
            _speechConfig.SetProfanity(ProfanityOption.Raw); // Raw as filtered (***) is useless input
            _pronunciationAssessmentConfig = new PronunciationAssessmentConfig(string.Empty,  // Empty string as no topic defined beforehand
                GradingSystem.HundredMark, Granularity.Word);
            _pronunciationAssessmentConfig.EnableProsodyAssessment();
        }

        /// <summary>
        /// Streams audio from a WebSocket to the speech recognition service and returns the pronounciation assessment result.
        /// The user gets progress updates via the WebSocket.
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="language"></param>
        /// <returns>Either the pronounciation assessment result or null on errors or if the WebSocket was closed before the speech recognition finished.</returns>
        public async Task<SpeechRecognitionResult?> StreamFromWebSocket(WebSocket ws, string language)
        {
            _logger.LogDebug($"Starting from WebSocket with language {language}");
            var buffer = new byte[AppConfig.SpeechWsBuffer];

            using var audioInputStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetDefaultInputFormat());
            using var audioConfig = AudioConfig.FromStreamInput(audioInputStream);
            using var recognizer = new SpeechRecognizer(_speechConfig, language, audioConfig);

            SpeechRecognitionResult? recognitionResult = null;
            string? error = null;
            InitRecognizer(language, recognizer, ws, (r) => recognitionResult ??= r, (e) => error ??= e);
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

            try
            {
                while (ws.State == WebSocketState.Open && recognitionResult == null && error == null)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Binary:
                            _logger.LogDebug($"Received {result.Count} bytes from WebSocket");
                            if (result.Count == 1)
                            {
                                _logger.LogDebug("One byte received, manual end signal.");
                                await recognizer.StopContinuousRecognitionAsync();
                            }
                            else
                            {
                                audioInputStream.Write(buffer.AsSpan(0, result.Count).ToArray());
                            }
                            break;
                        case WebSocketMessageType.Close:
                            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
                            _logger.LogDebug("WebSocket closed by client");
                            break;
                        default:
                            _logger.LogWarning("Received unexpected message from WebSocket");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while reading from WebSocket");
            }

            _logger.LogDebug("Done with Speech Recognition.");
            return recognitionResult;
        }

        private void InitRecognizer(string language, SpeechRecognizer recognizer, WebSocket ws, Action<SpeechRecognitionResult> onResult, Action<string> onError)
        {
            _pronunciationAssessmentConfig.ApplyTo(recognizer);

            recognizer.Recognizing += async (s, e) =>
            {
                _logger.LogDebug($"Recognizing: {e.Result.Text}");
                var partialText = JsonSerializer.Serialize(new SocketResult<string>(e.Result.Text, SocketResultType.PartialSpeech));
                await WebSocketHelper.SendTextWhenOpen(ws, partialText);
            };

            recognizer.Recognized += async (s, e) =>
            {
                _logger.LogDebug($"Speech recognition result: {e.Result.Reason}, {e.Result.Text}");

                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    onResult(e.Result);
                    await recognizer.StopContinuousRecognitionAsync(); // Stop recognition after first result to exit more quickly
                    var pronounciation = PronunciationAssessmentResult.FromResult(e.Result);
                    var result = new SpeechPronounciationResult(e.Result.Text,
                        language,
                        pronounciation.AccuracyScore,
                        pronounciation.FluencyScore,
                        pronounciation.CompletenessScore,
                        pronounciation.PronunciationScore,
                        pronounciation.Words.Select(w => new Word(w.Word, w.AccuracyScore, w.ErrorType)).ToList()
                    );
                    // User gets full result
                    await WebSocketHelper.SendTextWhenOpen(ws, JsonSerializer.Serialize(new SocketResult<SpeechPronounciationResult>(result, SocketResultType.SpeechResult)));
                }
                else
                {
                    _logger.LogDebug($"Did not recognize speech: '{e.Result.Reason}");
                    var errorResponse = new ErrorResponse("Could not recognize speech. The recording might be too short.");
                    await WebSocketHelper.SendTextWhenOpen(ws, JsonSerializer.Serialize(new SocketResult<ErrorResponse>(errorResponse, SocketResultType.Error)));
                }
            };

            recognizer.Canceled += (s, e) =>
            {
                if (e.Reason == CancellationReason.Error)
                {
                    _logger.LogError($"Speech recognition canceled: {e.Reason}, {e.ErrorDetails}");
                    onError(e.ErrorDetails);
                }
            };
        }
    }
}
