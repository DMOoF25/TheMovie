# Operation Contract - Opret biograf
Operation: CreateCinema(name, address)

## Formål:
At oprette og gemme en ny biograf i systemet, så den kan bruges til planlægning af filmvisninger.

Knytter til Use Case: UC1 – Opret biografer

## Prækonditioner
Systemet er startet og databasen er tilgængelig.

Biografens navn og adresse er udfyldt.

## Postkonditioner

En ny biografinstans er oprettet i systemets biografliste.

Biografen er gemt i persistent storage.

Biografen er synlig i UI.

Hvis gemning mislykkes, informeres brugeren, og systemet ruller tilbage til tidligere tilstand.