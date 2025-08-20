# Operation Contract – Opret månedsprogram

Operation: CreateMonthlyProgram(month, year)

## Formål:
At oprette og gemme et komplet filmprogram for den kommende måned, inkl. beregning af samlet spilletid (film + 15 min reklamer + 15 min rengøring), så programmet kan anvendes af biograferne.

Knytter til Use Case: UC-2 Opret månedsprogram

## Prækonditioner

Systemet er startet (og eksisterende film- og saldata er indlæst fra persistent storage).

month og year er gyldigt valgt.

Der findes mindst én film i systemets database.

Hver biografsal er registreret i systemet.

## Postkonditioner

Programmet for den valgte måned er oprettet i systemets interne programliste.

For hver forestilling er spilletid udregnet som: filmvarighed + 15 min reklamer + 15 min rengøring.

Systemet har valideret, at der ikke er overlap i spilletider i samme sal.

Programmet er gemt i persistent storage.

UI viser det færdige månedsprogram, klar til eksport.

Hvis gemningen mislykkes, informeres brugeren, og systemet ruller tilbage til tidligere tilstand.