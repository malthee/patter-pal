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
            _pronunciationAssessmentConfig = new PronunciationAssessmentConfig("",  // Empty string as no topic defined beforehand
                GradingSystem.HundredMark, Granularity.Word);
            _pronunciationAssessmentConfig.EnableProsodyAssessment();
        }

        public async Task StartFromWebSocket(WebSocket ws)
        {
            _logger.LogDebug("SpeechPronounciationService - Starting from WebSocket");
            var buffer = new byte[AppConfig.SpeechWsBuffer];

            // THIS IS PROBLEM, as mediarecorder does not give us PCM :c
            using var audioInputStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetDefaultInputFormat());
            using var audioConfig = AudioConfig.FromStreamInput(audioInputStream);
            using var recognizer = new SpeechRecognizer(_speechConfig, "en-US" /*TODO*/, audioConfig);

            InitRecognizer(recognizer, ws);
            await recognizer.StartContinuousRecognitionAsync();

            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    audioInputStream.Write(new byte[0]); // End stream signal
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    _logger.LogDebug("SpeechPronounciationService - WebSocket closed by client");
                }
                else
                {
                    _logger.LogDebug($"SpeechPronounciationService - Received {result.Count} bytes from WebSocket");
                    audioInputStream.Write(buffer.AsSpan(0, result.Count).ToArray());
                }
            }

            await recognizer.StopContinuousRecognitionAsync();
        }

        private void InitRecognizer(SpeechRecognizer recognizer, WebSocket ws)
        {
            _pronunciationAssessmentConfig.ApplyTo(recognizer);

            recognizer.Recognizing += (s, e) =>
            {
                _logger.LogDebug($"Recognizing: {e.Result.Text}");
            };

            recognizer.Recognized += async (s, e) =>
            {
                _logger.LogDebug($"Speech recognition result: {e.Result.Reason}, {e.Result.Text}");
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    var pronunciationResultJson = e.Result.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);
                    _logger.LogDebug($"Recognized: {pronunciationResultJson}");
                    await SendResultToClient(ws, pronunciationResultJson);
                }
            };

            recognizer.Canceled += (s, e) =>
            {
                _logger.LogError($"Speech recognition canceled: {e.Reason}, {e.ErrorDetails}");
            };
        }

        private async Task SendResultToClient(WebSocket ws, string message)
        {
            var messageBuffer = Encoding.UTF8.GetBytes(message);
            await ws.SendAsync(new ArraySegment<byte>(messageBuffer, 0, messageBuffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
