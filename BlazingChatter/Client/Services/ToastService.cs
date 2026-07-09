namespace BlazingChatter.Client.Services;

public interface IToastService
{
    event Action? OnChange;

    IReadOnlyCollection<ToastMessage> Toasts { get; }

    void Show(string title, string? description = null, ToastKind kind = ToastKind.Info);

    void Remove(string id);
}

public sealed class ToastService : IToastService
{
    readonly List<ToastMessage> _toasts = new();
    static readonly TimeSpan s_lifetime = TimeSpan.FromSeconds(4);

    public event Action? OnChange;

    public IReadOnlyCollection<ToastMessage> Toasts => _toasts;

    public void Show(
        string title, string? description = null, ToastKind kind = ToastKind.Info)
    {
        var toast = new ToastMessage(Guid.NewGuid().ToString("N"), title, description, kind);
        _toasts.Add(toast);
        OnChange?.Invoke();

        _ = DismissLaterAsync(toast.Id);
    }

    public void Remove(string id)
    {
        if (_toasts.RemoveAll(toast => toast.Id == id) > 0)
        {
            OnChange?.Invoke();
        }
    }

    async Task DismissLaterAsync(string id)
    {
        await Task.Delay(s_lifetime);
        Remove(id);
    }
}
