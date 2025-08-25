Operation Contract (OC)
Operation: CreateBooking(showtimeId, tickets, email, phone)

Formål:
At oprette en ny booking, hvis billetantal ikke overskrider salens kapacitet, og gemme den i persistent storage.

Knytter til Use Case: UC-1 Opret booking

Prækonditioner

Systemet er startet og har indlæst programdata og salenes kapacitet.

Kunde har valgt film, dato, biograf (dvs. et showtimeId).

tickets > 0.

email og phone er ikke tomme og valideret.

Postkonditioner

En ny booking er oprettet og gemt i persistent storage.

Systemet har reduceret antal ledige billetter i salen.

Kunden får en bekræftelse.

Hvis billetantal > salens kapacitet → booking gemmes ikke, kunden informeres.