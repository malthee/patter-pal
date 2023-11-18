using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using Microsoft.CognitiveServices.Speech;
using patter_pal.Util;
using System.Net.WebSockets;
using System.Text;

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
            _pronunciationAssessmentConfig = new PronunciationAssessmentConfig(string.Empty,  // Empty string as no topic defined beforehand
                GradingSystem.HundredMark, Granularity.Word);
            _pronunciationAssessmentConfig.EnableProsodyAssessment();
        }

        // TODO stop when specific length is reached
        public async Task StartFromWebSocket(WebSocket ws, string language)
        {
            _logger.LogDebug($"Starting from WebSocket with language {language}");
            var buffer = new byte[AppConfig.SpeechWsBuffer];

            using var audioInputStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetDefaultInputFormat());
            using var audioConfig = AudioConfig.FromStreamInput(audioInputStream);
            using var recognizer = new SpeechRecognizer(_speechConfig, language, audioConfig);
            using var cts = new CancellationTokenSource();

            InitRecognizer(recognizer, ws);
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

            try
            {
                while (ws.State == WebSocketState.Open)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None); // Always receive, only stop when client closes or timeout occurs

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Binary:
                            _logger.LogDebug($"Received {result.Count} bytes from WebSocket");
                            if (result.Count == 1)
                            {
                                _logger.LogDebug("One byte received end signal.");
                                await recognizer.StopContinuousRecognitionAsync();
                            }

                            audioInputStream.Write(buffer.AsSpan(0, result.Count).ToArray());
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
            finally
            {
                // If not stopped already, do it now
                await recognizer.StopContinuousRecognitionAsync();
            }
        }

        private void InitRecognizer(SpeechRecognizer recognizer, WebSocket ws)
        {
            _pronunciationAssessmentConfig.ApplyTo(recognizer);

            recognizer.Recognizing += async (s, e) =>
            {
                var pronunciationResultJson = e.Result.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);
                await SendResultToClient(ws, pronunciationResultJson);
                _logger.LogDebug($"Recognizing: {e.Result.Text}");       
            };

            recognizer.Recognized += async (s, e) =>
            {
                _logger.LogDebug($"Speech recognition result: {e.Result.Reason}, {e.Result.Text}");
                var pronounciationResult = PronunciationAssessmentResult.FromResult(e.Result);
                var pronunciationResultJson = e.Result.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);

                if (e.Result.Reason != ResultReason.RecognizedSpeech)
                {
                    _logger.LogDebug($"Did not recognize speech: '{e.Result.Text}', {e.Result.Reason}");
                }

                await SendResultToClient(ws, pronunciationResultJson);
            };

            recognizer.Canceled += (s, e) =>
            {
                _logger.LogError($"Speech recognition canceled: {e.Reason}, {e.ErrorDetails}");
                ws.Abort();
            };
        }

        private async Task SendResultToClient(WebSocket ws, string message)
        {
            if (ws.State == WebSocketState.Open)
            {
                var messageBuffer = Encoding.UTF8.GetBytes(message);
                await ws.SendAsync(new ArraySegment<byte>(messageBuffer, 0, messageBuffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
