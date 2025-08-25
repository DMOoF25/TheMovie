Operation: EditBooking(bookingId, newTickets)

Formål:
At ændre antallet af billetter i en eksisterende booking, hvis kunden ønsker flere eller færre billetter, og sikre, at salens kapacitet ikke overskrides.

Knytter til Use Case: UC-4 Rediger booking

Prækonditioner

Systemet er startet.

Booking med bookingId eksisterer i persistent storage.

Den ansatte har korrekt adgang til at redigere booking.

Nye antal billetter (newTickets) > 0.

Postkonditioner

Booking er opdateret med nyt antal billetter.

Salens ledige pladser opdateres korrekt.

UI viser bekræftelse til den ansatte.

Hvis ændringen ikke er mulig pga. kapacitet → booking forbliver uændret og medarbejder informeres.