using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using TheMovie.Application.Abstractions;
using TheMovie.Domain.Entities;

namespace TheMovie.UI.ViewModels;

public class HallListItemViewModel
{
    private static readonly ConcurrentDictionary<Guid, string> _cinemaNameCache = new();

    public Guid Id { get; }
    public string Name { get; }
    public uint Capacity { get; }
    public Guid CinemaId { get; }
    public string CinemaNameDisplay { get; } = string.Empty;

    public HallListItemViewModel(Hall hall)
    {
        Id = hall.Id;
        Name = hall.Name;
        Capacity = hall.Capacity;
        CinemaId = hall.CinemaId;

        // Resolve cinema name (cached lookups to avoid repeated repository calls)
        if (hall.CinemaId != Guid.Empty)
        {
            if (!_cinemaNameCache.TryGetValue(hall.CinemaId, out var name))
            {
                try
                {
                    var repo = App.HostInstance.Services.GetRequiredService<ICinemaRepository>();
                    var cinema = repo.GetByIdAsync(hall.CinemaId).GetAwaiter().GetResult();
                    name = cinema?.Name ?? string.Empty;
                }
                catch
                {
                    name = string.Empty;
                }
                CinemaNameDisplay = name ?? string.Empty;
            }
        }
        else
        {
            CinemaNameDisplay = string.Empty;
        }
    }
}