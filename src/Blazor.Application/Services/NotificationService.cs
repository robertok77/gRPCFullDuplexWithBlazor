using System.ComponentModel;
using System.Runtime.CompilerServices;
using gRPCFullDuplex.Blazor.Application.Contract;

namespace gRPCFullDuplex.Blazor.Application.Services;

public class NotificationService : INotificationService
{
    private string _errorMessage = "";
    
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetField(ref _errorMessage, value);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public void Clear()
    {
        SetField(ref _errorMessage, String.Empty);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
