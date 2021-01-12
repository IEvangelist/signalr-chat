using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazingChatter.Client.Interop
{
    public static class JavaScriptExtensions
    {
        public static async ValueTask SpeakAsync(
            this IJSRuntime javaScript, string message, string defaultVoice, double voiceSpeed, string lang) =>
            await javaScript.InvokeVoidAsync("app.speak", message, defaultVoice, voiceSpeed, lang);

        public static async ValueTask NotifyAsync(
            this IJSRuntime javaScript, string title, string message) =>
            await javaScript.InvokeVoidAsync("app.notify", title, message);

        public static async ValueTask ScrollIntoViewAsync(
            this IJSRuntime javaScript) =>
            await javaScript.InvokeVoidAsync("app.updateScroll");

        public static async ValueTask FocusElementAsync(
            this IJSRuntime javaScript, string elementId) =>
            await javaScript.InvokeVoidAsync("app.focus", elementId);

        public static async ValueTask<string> GetClientVoices<T>(
            this IJSRuntime javaScript,
            T instance) where T : class =>
            await javaScript.InvokeAsync<string>(
                "app.getClientVoices", DotNetObjectReference.Create(instance));

    }
}
