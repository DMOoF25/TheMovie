# User Story: Tilføj film
Som biograf ejer vil jeg kunne tilføje en ny film med relevante oplysninger, så filmen bliver synlig i systemets filmliste.

## Forretningsværdi
Sikrer hurtig og korrekt publicering af film, hvilket understøtter billet­salg og planlægning.

## Acceptkriterier (liste)
1. Jeg kan åbne en “Tilføj film” formular fra filmoversigten.
1. Jeg kan indtaste minimum: Titel (obligatorisk), Genre (vælges fra liste), Spilletid (i minutter), Premiere dato, Censur/aldersgrænse.
1. Titel og Genre er obligatoriske og valideres før gem.
1. Spilletid skal være et positivt heltal (> 0).
1. Ved valideringsfejl vises tydelige fejlmeddelelser under/ved feltet og gemmehandling afbrydes.
1. Ved succesfuld gemning vises en bekræftelse.
1. Den nye film vises øverst eller efter sorteringsregler i filmoversigten uden reload af hele siden (hvis SPA) eller efter redirect (hvis klassisk).
1. Data gemmes persistent.
1. Formularen kan annulleres uden at data gemmes.

## Ekstra regler / noter
- Genre vælges fra eksisterende genreliste; ingen fri tekst.
- Understøt flere genrer (afkrydsning).

## Gherkin scenarier

### Scenario 1: Succesfuld oprettelse
**Given** jeg er på filmoversigten  
**When** jeg klikker "Tilføj film"  
**And** jeg udfylder "Titel" med "Stjernerejse"  
**And** jeg vælger "Genre" = "Science Fiction"  
**And** jeg udfylder "Spilletid" med "128"  
**And** jeg klikker "Gem"  
**Then** ser jeg en bekræftelse  
**And** filmen "Stjernerejse" vises i filmoversigten  
**And** data er gemt i systemet  

### Scenario 2: Manglende obligatoriske felter
**Given** jeg åbner “Tilføj film” formularen  
**When** jeg lader "Titel" være tom  
**And** jeg klikker "Gem"  
**Then** vises en fejlmeddelelse ved feltet "Titel"  
**And** filmen gemmes ikke

### Scenario 3: Ugyldig spilletid
**Given** jeg udfylder alle felter korrekt undtagen "Spilletid"  
**When** jeg angiver "Spilletid" = "0"  
**And** jeg klikker "Gem"  
**Then** ser jeg en fejlmeddelelse “Spilletid skal være > 0”  
**And** filmen gemmes ikke  

### Scenario 5: Annullering
**Given** jeg har indtastet data i formularen  
**When** jeg klikker "Annuller"  
**Then** lukkes formularen  
**And** ingen data gemmes  

## Ikke-funktionelle krav
- Validering sker både klient-side og server-side.  
- Respons på gem ≤ 2 sekunder under normal belastning.  
- Felter følger tilgængelighedskrav (labels, ARIA hvor relevant).  

## Åben afklaring
- Skal der være felt for sprog / undertekster?  

## Definition of Done (uddrag)
- Alle acceptkriterier dækket af automatiske tests (unit + mindst 1 integration). 
- Gherkin scenarier mappet til tests.  
- Model- og inputvalidering implementeret.  
- Persistens verificeret.  
- UI gennemgået for tilgængelig label-tilknytning.  