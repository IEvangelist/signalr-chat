using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazingChatter.Client.Interop
{
    public static class JavaScriptExtensions
    {
        public static async ValueTask SpeakAsync(
            this IJSRuntime javaScript, string message, string defaultVoice, int voiceSpeed) =>
            await javaScript.InvokeVoidAsync("speak", message, defaultVoice, voiceSpeed);

        public static async ValueTask NotifyAsync(
            this IJSRuntime javaScript, string title, string message) =>
            await javaScript.InvokeVoidAsync("notify", title, message);
    }
}
