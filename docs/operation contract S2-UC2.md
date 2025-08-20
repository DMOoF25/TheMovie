# Operation Contract - Opret biografsal
Operation: CreateCinemaHall(cinemaID, hallName, capacity)

## Formål:
At oprette og gemme en ny sal i en eksisterende biograf, så den kan tildeles filmvisninger.

Knytter til Use Case: UC2 – Opret biografsale

## Prækonditioner
Systemet er startet og databasen er tilgængelig.

Den valgte biograf (cinemaID) eksisterer i systemet.

hallName og capacity er udfyldt korrekt.

## Postkonditioner

En ny sal er oprettet og knyttet til den valgte biograf.

Salen er gemt i persistent storage.

Salen er synlig i UI under biografens sal-oversigt.

Hvis gemning mislykkes, informeres brugeren, og systemet ruller tilbage til tidligere tilstand.