Operation: DeleteBooking(bookingId)

Formål:
At slette en eksisterende booking i systemet, hvis kunden eller biografen ønsker at aflyse, og opdatere salens ledige pladser.

Knytter til Use Case: UC-3 Slet booking

Prækonditioner

Systemet er startet.

Booking med bookingId eksisterer i persistent storage.

Den ansatte er logget ind med korrekt adgang.

Postkonditioner

Booking fjernes fra systemets database.

Salens ledige pladser opdateres korrekt.

UI viser bekræftelse til den ansatte.

Hvis sletning mislykkes → bruger informeres og systemets tilstand rulles tilbage.