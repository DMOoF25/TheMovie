<!-- BADGES V1 -->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![downloads][downloads-shield]][downloads-url]
[![Issues][issues-shield]][issues-url]
# TheMovie
Demonstrate HLD and LLD for a simple WPF application using MVVM, Dependency Injection, and the Repository Pattern.

[![Screenshot-menu-admin][Screenshot-menu-admin]][Screenshot-menu-admin-url]
[![Screenshot-editMovieView][Screenshot-editMovieView]][Screenshot-editMovieView-url]
[![Screenshot-editScreeningView][Screenshot-editScreeningView]][Screenshot-editScreeningView-url]

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

- Real persistence (SQLite / EF Core) for multiuser.
- Unit tests for UI click action.

## Project documentation

### Glossery

See the [Glossery](https://github.com/DMOoF25/TheMovie/blob/master/docs/OrdListe.md) for a Danish to English term list.


<!-- ALL LINKS & IMAGES SHORTCUT ONLY HAVE EFFECTS WHEN THE REPO IS PUBLISH ACCESS -->

<!-- MARKDOWN LINKS & IMAGES -->
[contributors-shield]: https://img.shields.io/github/contributors/DMOoF25/TheMovie?style=for-the-badge
[contributors-url]: https://github.com/DMOoF25/TheMovie/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/DMOoF25/TheMovie?style=for-the-badge
[forks-url]: https://github.com/DMOoF25/TheMovie/network/members
[stars-shield]: https://img.shields.io/github/stars/DMOoF25/TheMovie?style=for-the-badge
[stars-url]: https://github.com/DMOoF25/TheMovie/stargazers
[downloads-shield]: https://img.shields.io/github/downloads/DMOoF25/TheMovie/total?style=for-the-badge
[downloads-url]: https://github.com/DMOoF25/TheMovie/releases
[issues-shield]: https://img.shields.io/github/issues/DMOoF25/TheMovie?style=for-the-badge
[issues-url]: https://github.com/DMOoF25/TheMovie/issues
[license-shield]: https://img.shields.io/github/license/DMOoF25/TheMovie?style=for-the-badge
[license-url]: https://github.com/DMOoF25/TheMovie/blob/master/LICENSE
[Repos-size-shield]: https://img.shields.io/github/repo-size/DMOoF25/Dotnet.PfxCertificateManager?style=for-the-badge

[Glossery-url]: https://github.com/DMOoF25/TheMovie/blob/master/docs/OrdListe.md

[Screenshot-menu-admin]: https://raw.githubusercontent.com/DMOoF25/TheMovie/master/images/screenshots/small/Screenshot-menu-admin.png
[Screenshot-menu-admin-url]: https://github.com/DMOoF25/TheMovie/blob/master/images/screenshots/Screenshot-menu-admin.png
[Screenshot-editMovieView]: https://raw.githubusercontent.com/DMOoF25/TheMovie/master/images/screenshots/small/Screenshot-editMovieView.png
[Screenshot-editMovieView-url]: https://github.com/DMOoF25/TheMovie/blob/master/images/screenshots/Screenshot-editMovieView.png
[Screenshot-editScreeningView]: https://raw.githubusercontent.com/DMOoF25/TheMovie/master/images/screenshots/small/Screenshot-editScreeningView.png
[Screenshot-editScreeningView-url]: https://github.com/DMOoF25/TheMovie/blob/master/images/screenshots/Screenshot-editScreeningView.png
