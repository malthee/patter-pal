using System.Net.WebSockets;
using System.Text;

namespace patter_pal.Util
{
    public class WebSocketHelper
    {
        /// <summary>
        /// Sends a text message to the client if the WebSocket is still open.
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task SendTextWhenOpen(WebSocket ws, string message)
        {
            if (ws.State == WebSocketState.Open)
            {
                var messageBuffer = Encoding.UTF8.GetBytes(message);
                await ws.SendAsync(new ArraySegment<byte>(messageBuffer, 0, messageBuffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
