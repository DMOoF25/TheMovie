# TheMovie
Demonstrate HLD and LLD for a simple WPF application using MVVM, Dependency Injection, and the Repository Pattern.

## Solution Structure

```plaintext
src/
    TheMovie.Domain/
        Entities/               # Core domain entity types (e.g., Movie)
        ValueObjects/           # Immutable value objects (e.g., Duration, if added) (Other domain)
                                # Pure business logic; no UI or infra dependencies
    TheMovie.Application/
        Abstractions/           # Interfaces (e.g., IMovieRepository) used by higher layers
        DependencyInjection.cs  # Extension method to register application services
    TheMovie.Infrastructure/
        Data/                   # In-memory repository implementations
        DependencyInjection.cs  # Registers infrastructure services (repositories)
    TheMovie.UI/
        Views/                  # WPF Pages/Windows (AddMovieView, MainWindow)
        ViewModels/             # MVVM ViewModel classes (MovieViewModel, option VMs)
        Converters/             # WPF value converters (e.g., NullToVisibilityConverter)
        Commands/               # Reusable ICommand implementations (RelayCommand)
        App.xaml(.cs)           # WPF application bootstrapping + DI host setup
    tests/
    TheMovie.UI.Tests/
        Fakes/                  # Test doubles (e.g., FakeMovieRepository)
        ViewModels/             # Unit tests for ViewModels
```

## Key Concepts

- Dependency Injection: Configured in App.OnStartup via Host.CreateDefaultBuilder.
- Repository Pattern: In-memory generic base + concrete Movie repository.
- MVVM: ViewModels expose bindable state and ICommand instances.
- Validation: Simple command gating and input parsing (DurationText).
- Extensibility: Replace in-memory repo with persistent implementation by updating registrations.

## Roadmap Ideas

- Async repository calls end-to-end.
- Real persistence (SQLite / EF Core or Json file).
- Unit tests for converters and commands.