using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TheMovie.UI.ViewModels.Abstractions;

public abstract class ModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
