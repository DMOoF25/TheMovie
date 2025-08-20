# Use case 2: Opret månedsprogram

**Name:** Opret månedsprogram
**Primary actor:** Jens Peter (biograf ejer)
**Goal:** Oprette og gemme et fuldt filmprogram for den kommende måned til biograferne
**Level:** Bruger mål

## Stakeholders and interests

**Owner:**Owner (Jens Peter): 
Hurtig og korrekt planlægning uden overlap mellem forestillinger
Automatiseret beregning af spilletid inkl. reklamer og rengøring

**Audience/Staff:**
Få et klart og brugbart program til drift og information af publikum
Se præcise spilletider og premieredatoer

## Preconditions

**App state:** Applikationen kører; Programmodulet er tilgængeligt
**Data:** Film og biografsale er registreret i systemet. Genrer, instruktører og premieredatoer er tilgængelige

## Trigger
Action: Jens vælger ”Opret nyt månedsprogram” i systemet

## Success end condition
Result:
Månedsprogrammet er gemt og kan eksporteres til Excel/PDF
Ingen overlap i biografsale
Forestillingstider er korrekt beregnet (film + 15 min reklamer + 15 min rengøring)

## Failed end condition
Result:
Programmet gemmes ikke, hvis der mangler væsentlig information (fx film eller sal)
Jens informeres om fejl (fx overlap i tider, manglende data)