using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheMovie.Domain.Entities;

namespace TheMovie.UI.ViewModels;

public sealed class CinemaListItemViewModel : INotifyPropertyChanged
{
    public Guid Id { get; }
    public string Name { get; }
    public string Location { get; }

    public CinemaListItemViewModel(Cinema cinema)
    {
        Id = cinema.Id;
        Name = cinema.Name;
        Location = cinema.Location;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}