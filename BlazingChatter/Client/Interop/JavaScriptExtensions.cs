using Microsoft.JSInterop;

namespace BlazingChatter.Client.Interop;

public static class JavaScriptExtensions
{
    public static async Task NotifyAsync(
        this IJSRuntime javaScript, string title, string message) =>
        await javaScript.InvokeVoidAsync("app.notify", title, message);

    public static async ValueTask ScrollIntoViewAsync(
        this IJSRuntime javaScript) =>
        await javaScript.InvokeVoidAsync("app.updateScroll");
}
