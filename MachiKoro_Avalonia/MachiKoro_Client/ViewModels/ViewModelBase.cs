using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReactiveUI;

namespace MachiKoro_Client.ViewModels;

public class ViewModelBase : ReactiveObject, INotifyPropertyChanged
{
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}