# Operation Contract - Opret instruktør
Operation: CreateDirector(name, birthYear, nationality)

## Formål:
At oprette og gemme en instruktør, så denne kan knyttes til film i systemet.

Knytter til Use Case: UC3 – Opret instruktør

## Prækonditioner
Systemet er startet og databasen er tilgængelig.

Instruktørens navn er udfyldt.

## Postkonditioner

En ny instruktørpost er oprettet i systemets instruktørliste.

Instruktøren er gemt i persistent storage.

Instruktøren er synlig og kan vælges ved filmoprettelse/redigering.

Hvis gemning mislykkes, informeres brugeren, og systemet ruller tilbage til tidligere tilstand.