using System.ComponentModel;

namespace gRPCFullDuplex.Blazor.Application.Contract;

public interface INotificationService : INotifyPropertyChanged
{
    string ErrorMessage { get; set; }
    void Clear();
}