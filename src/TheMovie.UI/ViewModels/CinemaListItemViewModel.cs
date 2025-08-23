using TheMovie.Domain.Entities;

namespace TheMovie.UI.ViewModels;

public sealed class CinemaListItemViewModel
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
}