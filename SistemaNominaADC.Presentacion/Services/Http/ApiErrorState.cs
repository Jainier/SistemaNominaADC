namespace SistemaNominaADC.Presentacion.Services.Http;

public class ApiErrorState
{
    public string? Message { get; private set; }

    public event Action? OnChange;

    public void SetError(string? message)
    {
        Message = string.IsNullOrWhiteSpace(message) ? "Error desconocido." : message.Trim();
        NotifyStateChanged();
    }

    public void Clear()
    {
        if (string.IsNullOrWhiteSpace(Message))
            return;

        Message = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
