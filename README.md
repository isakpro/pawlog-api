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

Skickas ett inlägg utan rubrik svarar API:et `400` med ett valideringsfel, inte
`500`.

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

I utvecklingsläge finns även API-beskrivningen på
http://localhost:5005/openapi/v1.json.

## Databasen

Inläggen sparas i SQLite-filen `Pawlog.Api/pawlog.db`, som skapas automatiskt vid
start. Första gången läggs två exempelinlägg in så att webbappen visar något
direkt.

Vill du börja om från noll: stoppa API:et, radera `pawlog.db` och starta igen.

Filen är med i `.gitignore` – den är lokal och ska inte checkas in.

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

**CORS med `WithOrigins`, inte `AllowAnyOrigin`.** Bara kända klienter släpps in,
och adresserna ligger i konfigurationen så att de kan ändras utan omkompilering.

**Ingen https-redirect.** Webbappen anropar API:et över http i utvecklingsläge.
En redirect till https gör att anropen fastnar på ett självsignerat certifikat
innan de når controllern.

## Status

GET, POST och PUT fungerar mot databasen och CORS är på plats. Nästa steg är en
endpoint för filuppladdning, så att en bild kan kopplas till ett inlägg och
visas i webbappen.
