Use case 3: Slet booking
Navn: Slet en booking
Primær aktør: Ansat
Mål: Den ansatte skal kunne slette en eksisterende booking ved aflysning, og systemet skal opdatere salens ledige pladser korrekt.
Niveau: Brugermål

Interessenter og interesser

Ansat: Kan hurtigt aflyse booking uden fejl og give kunden korrekt information.

Biograf: Sikrer at kapacitet og systemdata altid er korrekte.

System: Skal validere, slette booking og opdatere ledige pladser.

Prækonditioner

Systemet kører, og bookingdata er indlæst i persistent storage.

Den ansatte har korrekt adgang til at slette bookinger.

Booking, der skal slettes, eksisterer i systemet.

Trigger

Den ansatte vælger ”Slet booking” for en specifik booking.

Succesbetingelse

Booking fjernes fra systemet.

Salens ledige pladser opdateres.

Den ansatte får bekræftelse på sletningen.

Fejlbetingelse

Booking slettes ikke, hvis der opstår en fejl i systemet (fx netværk, manglende adgang).

Den ansatte får en fejlbesked og kan forsøge igen.