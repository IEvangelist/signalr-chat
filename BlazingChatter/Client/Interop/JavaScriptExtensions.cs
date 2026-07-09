using Microsoft.JSInterop;

namespace BlazingChatter.Client.Interop;

public static class JavaScriptExtensions
{
    public static async ValueTask ScrollToBottomAsync(
        this IJSRuntime javaScript, string elementId) =>
        await javaScript.InvokeVoidAsync("app.scrollToBottom", elementId);
}
