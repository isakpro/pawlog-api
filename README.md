# Pawlog API

REST-API för hunddagboken Pawlog, byggt med ASP.NET WebAPI. Här sparas
dagboksinläggen: datum, rubrik, berättelse, träningsmål och en bild.

Det här repot innehåller **backend**. Den är en av tre delar i plattformen:

| Del | Teknik | Repo |
| --- | --- | --- |
| Webbapp | React + TypeScript | [WebappIsak](https://github.com/isakpro/WebappIsak) |
| Backend / API | ASP.NET WebAPI | det här repot |
| Mobilapp | React Native (Expo) | kommer |

## Kom igång

### Förutsättningar

- [.NET SDK 10](https://dotnet.microsoft.com/download) eller senare

Kontrollera att du har rätt version:

```bash
dotnet --version
```

### Starta API:et

```bash
git clone https://github.com/isakpro/pawlog-api.git
cd pawlog-api
dotnet run --project Pawlog.Api
```

API:et lyssnar på **http://localhost:5005**. Databasen skapas automatiskt vid
första starten, så det behövs ingen databasserver och inga migrationer.

Testa att det svarar:

```bash
curl http://localhost:5005/api/entries
```

Stoppa servern med `Ctrl + C`.

## Endpoints

| Metod | Väg | Gör | Svar |
| --- | --- | --- | --- |
| GET | `/api/entries` | Hämtar alla inlägg, nyast först | `200` med en lista |
| GET | `/api/entries/{id}` | Hämtar ett inlägg | `200`, eller `404` om det saknas |
| POST | `/api/entries` | Skapar ett inlägg | `201` med det skapade inlägget |
| PUT | `/api/entries/{id}` | Uppdaterar ett inlägg | `200`, eller `404` om det saknas |
| POST | `/api/entries/{id}/photo` | Laddar upp en bild till ett inlägg | `200` med inlägget, eller `404` om det saknas |

Skickas ett inlägg utan rubrik svarar API:et `400` med ett valideringsfel, inte
`500`. Samma sak gäller en bild som är tom, större än 5 MB eller inte är en
bild.

### Exempel

Skapa ett inlägg:

```bash
curl -X POST http://localhost:5005/api/entries \
  -H "Content-Type: application/json" \
  -d '{"date":"2026-09-16","title":"Lärde sig vänta vid dörren","story":"Satte sig utan att bli tillsagd.","trainingGoal":"Vänta innan vi går ut","goalCompleted":false}'
```

Bocka av träningsmålet på inlägg 1:

```bash
curl -X PUT http://localhost:5005/api/entries/1 \
  -H "Content-Type: application/json" \
  -d '{"date":"2026-09-16","title":"Lärde sig vänta vid dörren","story":"Satte sig utan att bli tillsagd.","trainingGoal":"Vänta innan vi går ut","goalCompleted":true}'
```

Ladda upp en bild till inlägg 1:

```bash
curl -X POST http://localhost:5005/api/entries/1/photo \
  -F "photo=@hund.jpg"
```

Svaret innehåller inlägget med en ifylld `photoUrl`, till exempel
`/uploads/3f1c....jpg`. Det är en relativ sökväg, så klienten sätter ihop den
med API-adressen: `http://localhost:5005/uploads/3f1c....jpg`.

I utvecklingsläge finns även API-beskrivningen på
http://localhost:5005/openapi/v1.json.

## Databasen

Inläggen sparas i SQLite-filen `Pawlog.Api/pawlog.db`, som skapas automatiskt vid
start. Första gången läggs två exempelinlägg in så att webbappen visar något
direkt.

Vill du börja om från noll: stoppa API:et, radera `pawlog.db` och starta igen.

Filen är med i `.gitignore` – den är lokal och ska inte checkas in.

## Bilder

Uppladdade bilder sparas som filer i `Pawlog.Api/uploads`, som skapas automatiskt
vid start. Själva inlägget har bara sökvägen till bilden, inte bilden i sig.

Bilderna hämtas sedan under `/uploads`, alltså
`http://localhost:5005/uploads/<filnamn>`. Byts en bild ut tas den gamla filen
bort.

Även den här mappen är med i `.gitignore`, eftersom den innehåller lokala
uppladdningar.

## CORS

Webbappen körs på `http://localhost:5173` och API:et på `http://localhost:5005`.
Eftersom portarna skiljer sig blockerar webbläsaren anropen om inte API:et
tillåter dem. Tillåtna adresser ligger i `appsettings.json`:

```json
"Cors": {
  "AllowedOrigins": ["http://localhost:5173", "http://127.0.0.1:5173"]
}
```

Kör du webbappen på en annan port lägger du till den i listan.

## Projektstruktur

```
Pawlog.Api/
  Controllers/   EntriesController - endpoints för inläggen
  Data/          PawlogDbContext och DatabaseSeeder
  Models/        DiaryEntry (databasmodell) och EntryRequest (inkommande data)
  Services/      PhotoStorage - sparar, validerar och tar bort bilder
  Program.cs     tjänster, CORS och pipeline
```

## Tekniska val

**Controllers istället för minimal API.** En fil per resurs gör det tydligt var
en endpoint hör hemma när API:et växer, och `[ApiController]` ger automatisk
modellvalidering med `400` och problem details utan extra kod.

**SQLite via EF Core.** Databasen är en vanlig fil i projektmappen. Den som
klonar repot behöver inte installera SQL Server eller köra migrationer – med
`EnsureCreated` räcker `dotnet run`. Samtidigt är det en riktig databas, så
inläggen finns kvar efter omstart.

**EntryRequest som egen modell.** Inkommande anrop går via `EntryRequest`
istället för `DiaryEntry`. Då kan en klient inte sätta `Id` eller `PhotoUrl` –
de ägs av API:et. Valideringsreglerna ligger som data annotations på modellerna
istället för som if-satser i varje endpoint.

**Bilder på disk, sökvägen i databasen.** Bilderna sparas som filer och
databasen håller bara sökvägen. Det gör att SQLite-filen inte växer med varje
uppladdning, och webbläsaren kan hämta och cacha bilden som vilken bild som
helst. Varje fil får ett nytt namn (en `Guid`), så att två bilder med samma
namn inte skriver över varandra och ett filnamn från en klient inte kan peka
utanför mappen. Bara kända bildformat tillåts.

**Uppladdning som egen endpoint.** `POST` och `PUT` tar emot JSON och bilden
går via `POST /api/entries/{id}/photo` som `multipart/form-data`. Då slipper
inläggen skickas som base64 i JSON, och en bild kan bytas utan att resten av
inlägget skickas om.

**Relativ `photoUrl`.** Databasen sparar `/uploads/<filnamn>` istället för en
full url. Byter API:et adress behöver inga rader i databasen ändras – klienten
sätter ihop sökvägen med den API-adress den redan har.

**CORS med `WithOrigins`, inte `AllowAnyOrigin`.** Bara kända klienter släpps in,
och adresserna ligger i konfigurationen så att de kan ändras utan omkompilering.

**Ingen https-redirect.** Webbappen anropar API:et över http i utvecklingsläge.
En redirect till https gör att anropen fastnar på ett självsignerat certifikat
innan de når controllern.

## Status

Alla endpoints fungerar mot databasen: lista, hämta, skapa, uppdatera och ladda
upp en bild. CORS är på plats för webbappen. Backend har därmed allt webbappen
behöver.

Nästa steg ligger i webbapp-repot: byta ut exempeldatan mot riktiga anrop hit,
ladda upp bilden till den nya endpointen och visa ett felmeddelande i
gränssnittet när ett anrop misslyckas.
