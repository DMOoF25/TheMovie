Operation Contract (OC)

Operation: FilterShowtimes(movie?, date?, cinema?)

Formål:
At søge i systemets programdatabase og returnere alle forestillinger, som matcher de ønskede filtre.

Knytter til Use Case: UC-4 Find dagens muligheder

Prækonditioner

Systemet er startet og har indlæst filmprogrammer fra persistent storage

Mindst ét filter er angivet (film, dato eller biograf)

Postkonditioner

En liste af mulige forestillinger er returneret til UI

Hvis listen er tom, gives en fejl-/info-besked til brugeren

Resultatet kan printes eller præsenteres for kunden