using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheMovie.Domain.ValueObjects;

namespace TheMovie.UI.ViewModels;

/// <summary>
/// Represents one selectable genre flag in the UI.
/// </summary>
public sealed class GenreOptionViewModel : INotifyPropertyChanged
{
    private bool _isSelected;

    public string Name { get; }
    public Genre Value { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public GenreOptionViewModel(string name, Genre value)
    {
        Name = name;
        Value = value;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? m = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(m));
}