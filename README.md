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
[![Screenshot-EditBookingView][Screenshot-EditBookingView]][Screenshot-EditBookingView-url]

## Indholdsfortegnelse
- [Om](#om)
- [Funktionalitet](#funktionalitet)
- [Teknologier](#teknologier)
- [Krav](#krav)
- [Centrale begreber](#centrale-begreber)
- [Roadmap-idéer](#roadmap-idéer)
- [Projektdokumentation](#projektdokumentation)
  - [HLD](#hld)
    - [Ordliste](#ordliste)
  - [LLD](#lld)
    - [Solution structure](#solution-structure)

## Om
TheMovie er et simpelt WPF-program til at administrere bestillinger af billettere og visning af film.
Det demonstrerer arkitekturprincipper som MVVM, Dependency Injection og Repository-mønsteret.

## Funktionalitet
- Liste, tilføj, rediger og slet film.
- Liste, tilføj, rediger og slet filminstruktør for film.
- Liste, tilføj, rediger og slet biograf.
- Liste, tilføj, rediger og slet sal for biograf.
- Liste, tilføj, rediger og slet visninger (screenings) for film.

## Teknologier
- WPF (.net 9)   # Det grafiske brugergrænsefladelag baseret på XAML.

## Krav
- Windows 10 eller nyere
- .NET 9 SDK eller nyere

## Installation
1. Klon dette repository: `git clone
1. Naviger til projektmappen: `cd TheMovie`
1. Kør powershell script for at hente eksempel data: `.\CopySampleData.ps1`
1. Byg og kør applikationen: `dotnet run --project src/TheMovie.UI/TheMovie.UI.csproj`
1. Brug applikationen til at administrere film, biografer og visninger.

## Centrale begreber

- Dependency Injection: Konfigureret i App.OnStartup via Host.CreateDefaultBuilder.
- Repository-mønster: In-memory generisk base + konkret Movie-repository.
- MVVM: ViewModels eksponerer bindbar tilstand og ICommand-instancer.
- Validering: Simpel kommando-gating og input-parsing (DurationText).
- Udvidbarhed: Udskift in-memory-repo med en persistent implementering ved at opdatere registreringer.

## Roadmap-idéer

- Reel persistens (SQLite / EF Core) til flere brugere.

## Projektdokumentation

### HLD

#### Ordliste

Se [Ordliste](https://github.com/DMOoF25/TheMovie/blob/master/docs/OrdListe.md) for en dansk-engelsk ordliste.


### LLD

#### Solution structure

```plaintext
src/
    TheMovie.Domain/            # Core domain logic
        Entities/               # Core domain entity types (e.g., Movie)
        ValueObjects/           # Immutable value objects (e.g., Genre, if added) (Other domain)
    TheMovie.Application/       # Pure business logic; no UI or infra dependencies
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
[Screenshot-EditBookingView]: https://raw.githubusercontent.com/DMOoF25/TheMovie/master/images/screenshots/small/Screenshot-bookingView.png
[Screenshot-EditBookingView-url]: https://github.com/DMOoF25/TheMovie/blob/master/images/screenshots/Screenshot-EditBookingView.png
