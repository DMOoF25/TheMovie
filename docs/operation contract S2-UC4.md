# Operation Contract - Opret spilletid
Operation: CreateShowtime(cinemaHallID, filmID, dateTime)

## Formål:
At oprette og gemme en spilletid for en film i en specifik sal, inkl. beregning af total spilletid (film + 15 min reklamer + 15 min rengøring).

Knytter til Use Case: UC4 – Opret spilletider

## Prækonditioner
Systemet er startet og databasen er tilgængelig.

Valgt film (filmID) eksisterer og har registreret varighed og premieredato.

Valgt sal (cinemaHallID) eksisterer.

dateTime er gyldigt og ikke i fortiden.

## Postkonditioner

En ny spilletid er oprettet i systemets programliste.

Spilletidens varighed er beregnet som filmens længde + 15 min reklamer + 15 min rengøring.

Systemet har valideret, at der ikke er overlap i samme sal.

Spilletiden er gemt i persistent storage.

UI viser spilletiden i kalender/oversigt.

Hvis gemning mislykkes, informeres brugeren, og systemet ruller tilbage til tidligere tilstand.