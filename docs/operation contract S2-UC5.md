# Operation Contract - Rediger film
Operation: EditMovie(movieID, newTitle, newGenre, newDuration, newDirectorID, newPremiereDate)

## Formål:
At opdatere oplysninger om en eksisterende film, så systemet altid viser de korrekte data.

Knytter til Use Case: UC5 – Rediger film

## Prækonditioner
Systemet er startet og databasen er tilgængelig.

Den valgte film (movieID) eksisterer i systemet.

Mindst ét felt er udfyldt med ny information.

## Postkonditioner

Filmens data er opdateret i systemets filmliste.

Ændringerne er gemt i persistent storage.

UI viser den opdaterede film.

Hvis gemning mislykkes, informeres brugeren, og systemet viser den tidligere version af filmen.