namespace BlazingChatter.Client.Services;

public enum ToastKind
{
    Info,
    Success,
    Warning,
    Error
}

public sealed record ToastMessage(
    string Id,
    string Title,
    string? Description,
    ToastKind Kind);
