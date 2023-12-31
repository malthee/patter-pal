﻿using Microsoft.CognitiveServices.Speech;
using patter_pal.domain.Config;
using patter_pal.Models;
using patter_pal.Util;
using System.Net.WebSockets;
using System.Text.Json;
using System.Web;

namespace patter_pal.Logic
{
    public class SpeechSynthesisService
    {
        private readonly ILogger<SpeechSynthesisService> _logger;
        private readonly AppConfig _appConfig;

        public SpeechSynthesisService(ILogger<SpeechSynthesisService> logger, AppConfig appConfig)
        {
            _logger = logger;
            _appConfig = appConfig;
        }

        /// <summary>
        /// Synthesizes text to speech and sends it to the client over <see cref="WebSocket"/>
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="text"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public async Task SendSynthesizedText(WebSocket ws, string text, string language)
        {
            _logger.LogDebug($"Starting Speech-Synthesis with language {language} and voice {_appConfig.SpeechSpeakerVoice}");
            var speechConfig = SpeechConfig.FromSubscription(_appConfig.SpeechSubscriptionKey, _appConfig.SpeechRegion);
            speechConfig.SetProfanity(ProfanityOption.Raw);
            //speechConfig.SpeechSynthesisVoiceName = _appConfig.SpeechSpeakerVoice;
            //speechConfig.SpeechSynthesisLanguage = language;

            // Preprocessing for IPA
            text = HttpUtility.HtmlEncode(text); // Escape for SSML
            text = WrapIpaWithSsmlTags(text);
            string ssml = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='{language}'>" +
                  $"<voice name='{_appConfig.SpeechSpeakerVoice}'>" +
                    $"{text}" +
                  $"</voice>" +
              $"</speak>";

            using var speechSynthesizer = new SpeechSynthesizer(speechConfig, null); // null to not speak audio on server
            using var result = await speechSynthesizer.SpeakSsmlAsync(ssml);

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                var audioData = result.AudioData;
                _logger.LogDebug($"Speech-Synthesis completed with {audioData.Length} bytes of audio data");
                if (ws.State == WebSocketState.Open)
                    await ws.SendAsync(new ArraySegment<byte>(audioData), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                _logger.LogWarning($"Synthesis cancelled: {cancellation.Reason}. Text: {ssml}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    _logger.LogError($"Synthesis error: {cancellation.ErrorCode} - {cancellation.ErrorDetails}");
                    var error = new ErrorResponse("Speech output is unavailable. Please try again later.", ErrorResponse.ErrorCode.SynthesisServiceError);
                    await WebSocketHelper.SendTextWhenOpen(ws, JsonSerializer.Serialize(new SocketResult<ErrorResponse>(error, SocketResultType.Error)));
                }
            }
        }

        private static string WrapIpaWithSsmlTags(string text)
        {
            return IpaHelper.ProcessIpa(text, (ipa) => $"<phoneme alphabet=\"ipa\" ph=\"{ipa}\">{ipa}</phoneme>");
        }
    }
}
