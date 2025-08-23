using TheMovie.Domain.Entities;

namespace TheMovie.UI.ViewModels;

public sealed class InstructorListItemViewModel
{
    public Guid Id { get; }
    public string Name { get; }

    public InstructorListItemViewModel(Instructor instructor)
    {
        Id = instructor.Id;
        Name = instructor.Name;
    }
}