README.md
Introduktion: SBA Pro Management System
Detta dokument beskriver de tekniska specifikationerna och implementeringsinstruktionerna för SBA Pro, ett komplett, molnbaserat SaaS-system (Software as a Service) för hantering av Systematiskt Brandskyddsarbete (SBA). Systemets syfte är att erbjuda en digital, effektiv och lagenlig lösning för fastighetsägare och verksamhetsutövare att uppfylla kraven i svensk lagstiftning, primärt Lagen (2003:778) om skydd mot olyckor (LSO).
Systemet är utformat för två primära användargrupper:
 * Administratörer: Systemadministratörer som hanterar flera kunder (tenanter), samt tenant-administratörer som ansvarar för sitt eget företags anläggningar, användare och brandskyddsdokumentation.
 * Kontrollanter/Brandskyddsansvariga: Personal som utför de praktiska kontrollronderna på plats, dokumenterar status på brandskyddsutrustning och rapporterar avvikelser.
Applikationen kommer att möjliggöra visuell hantering av kontrollronder genom interaktiva översiktsritningar, automatiserad rapportering och påminnelser, allt inom en säker och isolerad fleranvändararkitektur (multi-tenancy).
Teknisk Stack
Systemet kommer att utvecklas med en modern och robust teknisk stack, vald för sin prestanda, skalbarhet och starka ekosystem inom.NET.
 * Backend & Frontend:.NET (senaste versionen) med Blazor Server.
 * Databashantering: Entity Framework Core med SQLite för utveckling och initial driftsättning.
 * Kartfunktionalitet: Leaflet.js för interaktiv visning av icke-geografiska översiktsritningar.
 * PDF-generering: QuestPDF för att skapa professionella och lagenliga kontrollrapporter.
 * E-postnotifieringar: MailKit för att hantera automatiserade påminnelser och aviseringar.
Arkitektoniska Pelare
Arkitekturen är byggd på tre grundläggande principer för att säkerställa ett underhållbart, skalbart och resilient system.
 * Multi-Tenancy (Fleranvändararkitektur): Systemet är designat från grunden för att säkert isolera data mellan olika kunder (tenanter). Varje tenant har sin egen uppsättning av användare, anläggningar och brandskyddsdata, vilket garanterar konfidentialitet och integritet.
 * Clean Architecture: Lösningen följer principerna för Clean Architecture, med en tydlig separation mellan domänlogik (Core), infrastruktur (Infrastructure) och presentation (WebApp). Detta främjar testbarhet, underhållbarhet och flexibilitet att byta ut externa beroenden.
 * Resilient Offline-First Data Capture: En avancerad funktion för att hantera tillfälliga anslutningsavbrott. Kontrollanter kan fortsätta sitt arbete och registrera data även utan en stabil internetanslutning, med automatisk synkronisering när anslutningen återupprättas.
Repositorystruktur
Lösningen kommer att struktureras enligt följande för att upprätthålla en tydlig separation av ansvarsområden.
SBAPro.sln
└── src/
    ├── SBAPro.Core/
    │   ├── Entities/
    │   └── Interfaces/
    ├── SBAPro.Infrastructure/
    │   ├── Data/
    │   ├── Services/
    │   └── Migrations/
    └── SBAPro.WebApp/
        ├── Components/
        ├── Pages/
        ├── wwwroot/
        │   ├── js/
        │   └── css/
        └── Program.cs

 * SBAPro.Core: Innehåller applikationens kärna: domänentiteter (t.ex. Site, InspectionObject) och gränssnitt för tjänster. Detta projekt har inga externa beroenden utöver standardbiblioteken i.NET.
 * SBAPro.Infrastructure: Innehåller konkreta implementationer av gränssnitten från Core. Detta inkluderar databasaccess med Entity Framework Core, integration med e-posttjänster (MailKit) och andra externa system.
 * SBAPro.WebApp: Blazor Server-applikationen som utgör användargränssnittet. Denna del ansvarar för presentation och användarinteraktion.
Hur dessa instruktioner ska användas
Dessa Markdown-filer är avsedda att bearbetas sekventiellt av en AI-utvecklingsagent. Varje fil bygger på den föregående, från den grundläggande arkitekturen och datamodellen till implementeringen av specifika funktioner. Följ instruktionerna i den angivna ordningen för att säkerställa en korrekt och logisk uppbyggnad av systemet.

# 01-Architecture.md

## 1.1 Lösning och Projektuppsättning

Denna sektion specificerar de initiala stegen för att skapa lösningsstrukturen och de nödvändiga projekten med hjälp av.NET CLI. Strukturen är baserad på Clean Architecture-principerna.

Kör följande kommandon i en terminal för att skapa lösningsfilen och de tre huvudprojekten:bash
# Skapa en ny solution-fil
dotnet new sln -n SBAPro

# Skapa ett class library för Core-projektet
dotnet new classlib -n SBAPro.Core -o src/SBAPro.Core

# Skapa ett class library för Infrastructure-projektet
dotnet new classlib -n SBAPro.Infrastructure -o src/SBAPro.Infrastructure

# Skapa Blazor Server-applikationen
dotnet new blazorserver -n SBAPro.WebApp -o src/SBAPro.WebApp

# Lägg till projekten i solution-filen
dotnet sln SBAPro.sln add src/SBAPro.Core/SBAPro.Core.csproj
dotnet sln SBAPro.sln add src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj
dotnet sln SBAPro.sln add src/SBAPro.WebApp/SBAPro.WebApp.csproj

Därefter, etablera korrekta projektreferenser. Beroendeflödet ska vara WebApp -> Infrastructure -> Core.
# WebApp refererar till Infrastructure
dotnet add src/SBAPro.WebApp/SBAPro.WebApp.csproj reference src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj

# Infrastructure refererar till Core
dotnet add src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj reference src/SBAPro.Core/SBAPro.Core.csproj

1.2 NuGet-paketberoenden
För att systemet ska fungera korrekt krävs ett antal tredjepartsbibliotek. Tabellen nedan specificerar vilka NuGet-paket som ska installeras i respektive projekt. Detta säkerställer en konsekvent och reproducerbar utvecklingsmiljö.
| Projekt | Paketnamn | Version | Syfte |
|---|---|---|---|
| SBAPro.Core | - | - | Innehåller endast domänmodeller; inga externa paket. |
| SBAPro.Infrastructure | Microsoft.EntityFrameworkCore.Sqlite | Senaste | EF Core-provider för utvecklingsdatabasen. |
| SBAPro.Infrastructure | Microsoft.AspNetCore.Identity.EntityFrameworkCore | Senaste | EF Core-integration för ASP.NET Identity. |
| SBAPro.Infrastructure | MailKit | Senaste | Modernt bibliotek för att skicka e-postnotifieringar. |
| SBAPro.WebApp | QuestPDF | Senaste | Fluent API för att generera PDF-rapporter. |
| SBAPro.WebApp | Microsoft.EntityFrameworkCore.Tools | Senaste | För att köra EF Core-migreringar. |
Installera paketen med följande CLI-kommandon:
# Infrastructure-projektet
dotnet add src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/SBAPro.Infrastructure/SBAPro.Infrastructure.csproj package MailKit

# WebApp-projektet
dotnet add src/SBAPro.WebApp/SBAPro.WebApp.csproj package QuestPDF
dotnet add src/SBAPro.WebApp/SBAPro.WebApp.csproj package Microsoft.EntityFrameworkCore.Tools

1.3 Domänmodell och lagenlig efterlevnad
Domänmodellen är direkt härledd från de krav som ställs i svensk lagstiftning, specifikt Lagen om skydd mot olyckor (LSO 2003:778) och de allmänna råden i SRVFS 2004:3. Datamodellens mål är att digitalt fånga alla nödvändiga komponenter för en juridiskt tillräcklig SBA-dokumentation, vilket inkluderar organisation, rutiner, kontroller och uppföljning.
Följande C#-klasser/records ska definieras i SBAPro.Core/Entities-katalogen. Tabellen nedan fungerar som en ritning för databasschemat och klargör entiteternas ansvar och relationer.
| Entitet | Nyckelegenskaper | Relationer | Noteringar / Juridisk motivering |
|---|---|---|---|
| Tenant | Id, Name | Har många ApplicationUsers, Sites. | Representerar en kundorganisation. Grunden för dataisolering. |
| Site | Id, Name, Address, TenantId | Tillhör en Tenant. Har många FloorPlans. | En fysisk anläggning/byggnad som hanteras av en tenant. |
| FloorPlan | Id, Name, ImageData, ImageMimeType, SiteId | Tillhör en Site. Har många InspectionObjects. | Den visuella ytan för en kontrollrond. |
| InspectionObject | Id, TypeId, Description, NormalizedX, NormalizedY, FloorPlanId | Tillhör en FloorPlan. Har många InspectionResults. | En brandskyddsutrustning (t.ex. brandsläckare). Koordinaterna är normaliserade. |
| InspectionRound | Id, SiteId, InspectorId, StartedAt, CompletedAt, Status | Tillhör en Site. Har en ApplicationUser (Kontrollant). Har många InspectionResults. | Representerar en enskild, dokumenterad kontrollrond. |
| InspectionResult | Id, RoundId, ObjectId, Status, Comment, Timestamp | Tillhör en InspectionRound och ett InspectionObject. | Registreringen av en enskild kontroll av ett enskilt objekt. Detta är kärnbeviset för utfört SBA. |
| InspectionObjectType | Id, Name, Icon, TenantId | Tillhör en Tenant. | En konfigurerbar typ av kontroll-objekt (t.ex. "6kg pulversläckare"). |
Entitetsdefinitioner
// Plats: src/SBAPro.Core/Entities/Tenant.cs
public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<Site> Sites { get; set; } = new List<Site>();
}

// Plats: src/SBAPro.Core/Entities/Site.cs
public class Site
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public ICollection<FloorPlan> FloorPlans { get; set; } = new List<FloorPlan>();
}

//... (definiera resterande entiteter enligt tabellen ovan)

Lagstiftningen betonar att omfattningen av SBA ska anpassas efter den specifika verksamhetens risker. Ett sjukhus har helt andra risker och krav än ett litet kontor. Detta innebär att en statisk, "en-storlek-passar-alla"-modell för brandskyddsutrustning är juridiskt och funktionellt otillräcklig. Systemets arkitektur måste därför stödja tenant-specifika konfigurationer. Entiteten InspectionObjectType är ett direkt svar på detta krav. Genom att den är kopplad till en TenantId kan en administratör för "Sjukhus A" definiera objekttyper som "Röksektionsgräns" eller "Defibrillator", vilka är irrelevanta för "Kontor B". Denna flexibilitet är en kärnegenskap som lyfter systemet från en simpel datalogger till ett dynamiskt och anpassningsbart efterlevnadsverktyg.
1.4 Fleranvändararkitektur (Multi-Tenancy)
Strategival
För SBA Pro väljs en single-database, shared-table-strategi med en TenantId-diskriminatorkolumn. Detta är en pragmatisk och kostnadseffektiv metod för många SaaS-applikationer och har utmärkt stöd i Entity Framework Core. Varje entitet som är specifik för en kund kommer att ha en TenantId-kolumn, och EF Core kommer att använda ett globalt frågefilter för att automatiskt säkerställa att en användare endast kan se data som tillhör deras tenant.
Implementeringssteg
 * Entiteter och gränssnitt: Alla tenant-specifika entiteter (t.ex. Site, FloorPlan, InspectionObject) måste inkludera en public Guid TenantId { get; set; }-egenskap. Ett ITenantSpecific gränssnitt kan användas för att standardisera detta.
 * ITenantService: Skapa ett gränssnitt ITenantService i SBAPro.Core och en implementation TenantService i SBAPro.Infrastructure. Denna tjänst kommer att vara scoped och ansvarar för att identifiera den aktuella tenantens ID, vanligtvis från användarens autentiseringsanspråk (claims).
   // Plats: src/SBAPro.Core/Interfaces/ITenantService.cs
public interface ITenantService
{
    Guid GetTenantId();
}

 * DbContext-konfiguration: I SBAPro.Infrastructure/Data/ApplicationDbContext.cs, injicera ITenantService i konstruktorn.
   // Plats: src/SBAPro.Infrastructure/Data/ApplicationDbContext.cs
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ITenantService _tenantService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }
    // DbSet-egenskaper...
}

 * Globalt frågefilter: I ApplicationDbContext, överskugga OnModelCreating-metoden för att applicera ett HasQueryFilter på alla tenant-specifika entiteter. Detta är den centrala mekanismen för dataisolering.
   // I ApplicationDbContext.cs
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    var tenantId = _tenantService.GetTenantId();

    builder.Entity<Site>().HasQueryFilter(s => s.TenantId == tenantId);
    builder.Entity<InspectionObjectType>().HasQueryFilter(iot => iot.TenantId == tenantId);
    //... applicera filter för alla andra tenant-specifika entiteter
}

 * Tjänstregistrering: I SBAPro.WebApp/Program.cs, registrera DbContextFactory med en Scoped-livstid. Detta är avgörande för Blazor Servers tillståndsfulla, kretsbaserade modell. Det säkerställer att varje användarkrets får en DbContext-instans som är korrekt konfigurerad för just den användarens tenant, vilket förhindrar dataläckage mellan användarsessioner.
   // I Program.cs
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);


# 02-Admin-Module.md

## 2.1 Identitet och åtkomstkontroll

Systemets säkerhet och dataisolering bygger på en robust implementering av ASP.NET Core Identity. Detta ramverk kommer att användas för att hantera användare, lösenord, roller och autentisering.

### Konfiguration av Identity

Instruera agenten att konfigurera ASP.NET Core Identity med Entity Framework Core. Detta involverar att `ApplicationDbContext` ärver från `IdentityDbContext<ApplicationUser>`.

### Roller och behörigheter

Definiera följande applikationsroller för att styra åtkomst till olika delar av systemet:
*   **SystemAdmin**: Har fullständig kontroll över hela systemet. Denna roll är avsedd för ägaren av SaaS-tjänsten och kan skapa och hantera tenanter.
*   **TenantAdmin**: Har administrativ kontroll inom sin egen tenant. Kan hantera anläggningar (`Sites`), användare (`Inspectors`), översiktsritningar och kontroll-objektstyper för sin organisation.
*   **Inspector**: Slutanvändaren som utför kontrollronder. Har läs- och skrivbehörighet till kontrollronder och resultat inom sin tilldelade tenant.

### Utökad användarmodell

Standardanvändaren `IdentityUser` måste utökas för att koppla varje användare till en specifik tenant. Skapa en `ApplicationUser`-klass som ärver från `IdentityUser` och lägg till en `TenantId`.csharp
// Plats: src/SBAPro.Core/Entities/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public Guid? TenantId { get; set; } // Nullable för SystemAdmin
    public Tenant? Tenant { get; set; }
}

Initial dataseedning
Skapa en dataseedningsklass i Infrastructure-projektet för att säkerställa att systemet har en initial SystemAdmin-användare och de nödvändiga rollerna vid första uppstart.
2.2 Tenanthantering (SystemAdmin)
Detta är SaaS-leverantörens kontrollpanel. En SystemAdmin måste kunna hantera hela kundlivscykeln från detta gränssnitt.
Blazor-komponenter för tenanthantering
Skapa en uppsättning Blazor-komponenter under en Admin/Tenants-mapp i WebApp-projektet. Dessa sidor ska skyddas så att endast användare med rollen SystemAdmin kan komma åt dem.
 * TenantListPage.razor: Visar en tabell över alla befintliga tenanter med alternativ för att redigera eller se detaljer.
 * CreateTenantPage.razor: Ett formulär för att skapa en ny tenant. Formuläret ska innehålla fält för tenantens namn och uppgifter för att skapa det första TenantAdmin-kontot (användarnamn, e-post, lösenord). Vid skapandet av en ny tenant skapas både Tenant-entiteten och den associerade ApplicationUser-entiteten med rollen TenantAdmin.
2.3 Anläggnings- och ritningshantering (TenantAdmin)
Detta är kundens primära gränssnitt för att konfigurera sitt brandskyddsarbete. Varje åtgärd som utförs av en TenantAdmin måste vara strikt begränsad till deras egen TenantId. Denna dataisolering är inte en funktion som behöver implementeras i varje enskild komponent; den är en fundamental egenskap hos arkitekturen som säkerställs av det globala frågefiltret som definierades i 01-Architecture.md. En felaktighet i det filtret skulle innebära en katastrofal dataläcka, vilket gör testning av denna gräns till den absolut viktigaste testaktiviteten i hela projektet.
Blazor-komponenter för TenantAdmin
Skapa en uppsättning Blazor-komponenter under en Tenant-mapp i WebApp-projektet, skyddade för rollen TenantAdmin.
 * SiteManagementPage.razor: En sida för att utföra CRUD-operationer (Create, Read, Update, Delete) på Site-entiteter. Alla databasfrågor som hämtar Sites kommer automatiskt att filtreras av det globala frågefiltret, vilket garanterar att en TenantAdmin endast ser och kan hantera sina egna anläggningar.
 * FloorPlanManagementPage.razor: För en vald anläggning (Site), ska denna sida tillåta en TenantAdmin att hantera översiktsritningar (FloorPlans).
   * Uppladdning av ritning: Implementera en InputFile-komponent för att ladda upp bildfiler (t.ex. PNG, JPG, SVG).
   * Lagringsstrategi: För den initiala implementationen med SQLite lagras den uppladdade bilden som en byte-array direkt i FloorPlan-entiteten i databasen. Detta är enkelt och kräver ingen extern infrastruktur. Det är dock viktigt att notera att detta inte är en skalbar lösning för produktion. Arkitekturen bör utformas så att denna lagringsmekanism kan bytas ut mot en molnbaserad blob-lagringstjänst (t.ex. Azure Blob Storage eller AWS S3) i framtiden utan att kräva omfattande refaktorering av applikationslogiken. Detta kan uppnås genom att abstrahera fillagringen bakom ett gränssnitt (t.ex. IFileStorageService).

# 03-Inspection-Workflow.md

## 3.1 Leaflet.js-integration för icke-geografiska kartor

För att visualisera översiktsritningar och placera ut kontroll-objekt krävs ett flexibelt kartbibliotek som kan hantera anpassade bilder istället för geografiska kartor. Leaflet.js är ett utmärkt open source-val för detta ändamål tack vare dess stöd för anpassade koordinatsystem.[span_20](start_span)[span_20](end_span)[span_21](start_span)[span_21](end_span)

### JSInterop-tjänst

Skapa en C#-tjänst, `LeafletMapService.cs`, i `WebApp`-projektet och en motsvarande JavaScript-modul, `leafletMap.js`, i `wwwroot/js`. Denna arkitektur kapslar in all JavaScript-interaktion och ger en ren, starkt typad API till Blazor-komponenterna.

### Kartinitiering med `L.CRS.Simple`

I `leafletMap.js`, skapa en funktion för att initiera en Leaflet-karta på ett givet DOM-element. Använd `L.CRS.Simple` som koordinatsystem. Detta system behandlar kartan som ett enkelt kartesiskt plan (ett rutnät) där koordinaterna motsvarar pixlar, vilket är perfekt för att arbeta med en statisk bild.[span_22](start_span)[span_22](end_span)[span_23](start_span)[span_23](end_span)javascript
// Plats: wwwroot/js/leafletMap.js
export function initializeMap(mapId, imageDimensions) {
    const map = L.map(mapId, {
        crs: L.CRS.Simple,
        minZoom: -5
    });

    const bounds = [,];
    map.fitBounds(bounds);

    return map;
}

Bildöverlägg (ImageOverlay)
Skapa en funktion som lägger till översiktsritningen som ett bildlager med L.ImageOverlay. Bilddatan kommer att skickas från C# som en Base64-kodad data-URL. Lagrets gränser (bounds) ska baseras på bildens dimensioner för att etablera ett 1:1-förhållande mellan bildpixlar och kartkoordinater.
// I wwwroot/js/leafletMap.js
export function addImageOverlay(map, imageUrl, imageDimensions) {
    const bounds = [,];
    L.imageOverlay(imageUrl, bounds).addTo(map);
}

3.2 Modul för objektplacering (TenantAdmin)
Denna modul ger TenantAdmin verktygen för att digitalt förbereda en kontrollrond genom att placera ut markörer för brandskyddsutrustning på en översiktsritning.
Blazor-komponent: FloorPlanEditor.razor
Skapa en komponent som visar den valda översiktsritningen med hjälp av LeafletMapService.
Händelsehantering för klick
Implementera JSInterop för att fånga klickhändelser på kartan. När en administratör klickar på ritningen ska JavaScript-funktionen returnera de klickade [y, x]-koordinaterna till Blazor-komponenten. Leaflet använder [lat, lng] som standard, vilket i L.CRS.Simple motsvarar [y, x].
Skapande och rendering av markörer
 * Fånga koordinater: När Blazor-komponenten tar emot koordinaterna från ett klick, skapar den en ny InspectionObject-instans i minnet.
 * Modal dialog: En modal dialogruta öppnas där administratören kan välja typ av objekt (från de tenant-specifika InspectionObjectType-entiteterna) och ange en beskrivning.
 * Spara objekt: Vid bekräftelse sparas det nya InspectionObject till databasen.
 * Renderingslogik och koordinatnormalisering: Det är avgörande att systemet är resilient mot framtida ändringar av översiktsritningen, t.ex. om en ny version med högre upplösning laddas upp. Att spara absoluta pixelkoordinater () är skört. Istället ska koordinaterna normaliseras till relativa värden (procent av bildens höjd och bredd) innan de sparas i databasen. Exempelvis, för en 1000x800 bild blir  till [y: 0.5, x: 0.5].
 * Rendera markör: Efter att objektet har sparats anropas en JS-funktion för att rendera en anpassad markör (L.marker med ett L.icon) på kartan. Denna funktion måste ta emot de normaliserade koordinaterna och bildens nuvarande dimensioner för att kunna omvandla dem tillbaka till absoluta pixelkoordinater för korrekt placering.
3.3 Modul för kontrollrond (Inspector)
Detta är applikationens kärnfunktion där kontrollanten utför och dokumenterar sitt arbete.
Blazor-komponent: InspectionRound.razor
Skapa den huvudsakliga komponenten för kontrollronder.
Starta en rond
 * Kontrollanten väljer en anläggning (Site) och en översiktsritning.
 * Systemet laddar ritningen och alla tillhörande InspectionObject-markörer.
 * En ny InspectionRound-post skapas i databasen med status "Påbörjad".
Interaktiv checklista och visuell återkoppling
 * Interaktion: När kontrollanten klickar på en markör på kartan, visas en popup eller en sidopanel.
 * Statusregistrering: I detta gränssnitt kan kontrollanten ange status för objektet (t.ex. "OK", "Anmärkning", "Ej kontrollerad") och lägga till en kommentar. Varje interaktion skapar och sparar en ny InspectionResult-post i databasen, kopplad till den pågående InspectionRound och det specifika InspectionObject.
 * Visuell återkoppling: För att ge omedelbar överblick över rondens framsteg ska markörens ikon på kartan ändra färg efter att ett objekt har kontrollerats. Till exempel, grön för "OK" och röd för "Anmärkning". Detta ger kontrollanten en tydlig visuell indikation på vilka objekt som återstår att kontrollera.

# 04-Outputs.md

## 4.1 Generering av PDF-rapporter med QuestPDF

En central del av Systematiskt Brandskyddsarbete är dokumentationen. Den digitalt insamlade datan måste kunna exporteras till ett formellt, delbart format som uppfyller lagkraven.[span_31](start_span)[span_31](end_span)[span_32](start_span)[span_32](end_span)[span_33](start_span)[span_33](end_span) PDF-rapporten är den primära artefakten för att visa efterlevnad vid en eventuell tillsyn från kommunen.[span_34](start_span)[span_34](end_span)[span_35](start_span)[span_35](end_span) Dess utformning, tydlighet och korrekthet kan därför inte vara en eftertanke; den representerar det yttersta värdet av systemet för slutanvändaren.

### Tjänstintegration

1.  **Gränssnitt**: Definiera ett `IReportGenerator`-gränssnitt i `SBAPro.Core`.
2.  **Implementation**: Skapa en `QuestPdfReportGenerator`-klass i `SBAPro.Infrastructure` som implementerar gränssnittet.

### Datamodell och mall

1.  **Rapportmodell**: Skapa en `InspectionReportModel`-klass som aggregerar all data som behövs för en komplett rapport: information om anläggningen, detaljer om kontrollronden (datum, kontrollant), och en lista över alla `InspectionResult` med status, kommentarer och objektdetaljer.
2.  **QuestPDF-mall**: Skapa en detaljerad QuestPDF-dokumentmall, `InspectionReport.cs`. Använd QuestPDF:s fluent API för att designa en professionell rapport med följande sektioner [span_36](start_span)[span_36](end_span):
    *   **Sidhuvud**: Företagslogotyp (tenant-specifik), rapporttitel ("Protokoll från egenkontroll"), och anläggningens namn.
    *   **Sammanfattning**: Datum för ronden, namn på kontrollant, och en övergripande sammanfattning av resultatet (t.ex. antal anmärkningar).
    *   **Resultattabell**: En detaljerad tabell som listar varje kontrollerat objekt, dess placering/beskrivning, status ("OK"/"Anmärkning"), och eventuella kommentarer från kontrollanten.[span_37](start_span)[span_37](end_span)
    *   **Sidfot**: Sidnummer ("Sida X av Y") och datum för generering av rapporten.

### API-slutpunkt för nedladdning

Skapa en Minimal API-slutpunkt eller en controller-metod i `WebApp` som tar ett `InspectionRoundId` som parameter. Slutpunkten använder `IReportGenerator`-tjänsten för att generera PDF-dokumentet som en `byte`-array och returnerar det som en filnedladdning med `Content-Type: application/pdf`.[span_38](start_span)[span_38](end_span)

## 4.2 E-postnotifieringstjänst med MailKit

Automatiserade påminnelser är en viktig del av ett *systematiskt* brandskyddsarbete. Systemet ska kunna påminna ansvariga när det är dags för nästa kontrollrond.

### Rätt val av bibliotek

`System.Net.Mail.SmtpClient` är föråldrad och rekommenderas inte för ny utveckling. MailKit är det moderna, robusta och rekommenderade biblioteket för e-posthantering i.NET.[span_39](start_span)[span_39](end_span)[span_40](start_span)[span_40](end_span)

### Tjänstintegration och konfiguration

1.  **Gränssnitt**: Definiera ett `IEmailService`-gränssnitt i `SBAPro.Core`.
2.  **Implementation**: Skapa en `MailKitEmailService`-klass i `SBAPro.Infrastructure`.
3.  **Konfiguration**: Hantera SMTP-inställningar (server, port, användarnamn, lösenord) säkert via `appsettings.json` och ASP.NET Core Options Pattern. Undvik att hårdkoda känslig information.
4.  **Implementering**: Skapa en `SendEmailAsync`-metod som använder MailKits `SmtpClient` för att ansluta till SMTP-servern, autentisera, bygga ett `MimeMessage`-objekt (med avsändare, mottagare, ämne och brödtext) och skicka det.[span_41](start_span)[span_41](end_span)[span_42](start_span)[span_42](end_span)[span_43](start_span)[span_43](end_span)

### Användningsfall: Påminnelser

Implementera en `BackgroundService` i `WebApp`. Denna tjänst körs i bakgrunden och exekveras periodiskt (t.ex. en gång per dygn). Tjänsten frågar databasen efter anläggningar vars nästa planerade kontrollrond närmar sig sitt förfallodatum och använder `IEmailService` för att skicka en påminnelse till den ansvariga `TenantAdmin`.

# 05-Advanced.md

## 5.1 Implementering av "Resiliens vid tillfällig anslutningsförlust"

En standard Blazor Server-applikation är helt beroende av en konstant, låglatent anslutning till servern, eftersom all UI-logik och tillståndshantering sker på servern i en "krets".[span_44](start_span)[span_44](end_span) Om anslutningen bryts blir applikationen oanvändbar. För en kontrollant som arbetar i miljöer med dålig täckning, som källare eller stora betongbyggnader, är detta en oacceptabel begränsning.

Målet är inte att skapa en fullfjädrad offline-applikation (vilket är domänen för Blazor WebAssembly), utan att bygga en resilient lösning som kan **buffra datainmatning** under korta anslutningsavbrott och sedan **synkronisera** datan när anslutningen återupprättas. Detta skapar en hybridmodell för tillståndshantering mellan server och klient.

Arkitekturen förlitar sig på en tät integration mellan C# och JavaScript via JSInterop för att utnyttja webbläsarens `CacheStorage` API.[span_45](start_span)[span_45](end_span)[span_46](start_span)[span_46](end_span)

## 5.2 Klient-sidigt cachningslager

### `CacheStorageAccessor.js`

Skapa en komplett JavaScript-modul i `wwwroot/js` som exponerar funktioner för att interagera med `CacheStorage`.javascript
// Plats: wwwroot/js/cacheStorageAccessor.js

const CACHE_NAME = 'sba-pro-offline-data';

async function openCache() {
    return await window.caches.open(CACHE_NAME);
}

// Lagrar ett objekt (t.ex. ett inspektionsresultat) i cachen.
// Nyckeln är en unik URL som representerar resursen.
export async function store(key, data) {
    const cache = await openCache();
    const response = new Response(JSON.stringify(data));
    await cache.put(key, response);
}

// Hämtar alla lagrade objekt från cachen.
export async function getAll() {
    const cache = await openCache();
    const requests = await cache.keys();
    const results =;
    for (const request of requests) {
        const response = await cache.match(request);
        if (response) {
            const data = await response.json();
            results.push({ key: request.url, data: data });
        }
    }
    return results;
}

// Raderar ett specifikt objekt från cachen med dess nyckel.
export async function remove(key) {
    const cache = await openCache();
    await cache.delete(key);
}

CacheStorageService.cs
Skapa en C#-tjänst i WebApp som fungerar som en wrapper runt JSInterop-anropen till cacheStorageAccessor.js. Detta ger en starkt typad och lätthanterlig API för resten av applikationen.
// Plats: WebApp/Services/CacheStorageService.cs
public class CacheStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference _module;

    public CacheStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private async Task InitModule()
    {
        _module??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/cacheStorageAccessor.js");
    }

    public async Task StoreResultAsync(InspectionResult result)
    {
        await InitModule();
        var key = $"/api/inspection-results/{result.Id}";
        await _module.InvokeVoidAsync("store", key, result);
    }
    
    //... implementera metoder för getAll och remove
}

5.3 Synkroniseringslogik
Komplexiteten i denna funktion ligger i att hantera övergången mellan online- och offline-läge och att på ett tillförlitligt sätt synkronisera data.
Tillståndshantering i InspectionRound.razor
Komponenten måste modifieras för att vara medveten om anslutningsstatusen. Blazor Server tillhandahåller inbyggda mekanismer för detta, vanligtvis genom att injicera NavigationManager och hantera anslutningshändelser.
"Offline"-läge
 * Detektering: När Blazor-kretsen kopplas från, ska UI:t informera användaren men förbli interaktivt.
 * Lokal lagring: Istället för att anropa en servermetod för att spara ett InspectionResult, anropar komponenten nu CacheStorageService.StoreResultAsync för att spara resultatet lokalt i webbläsaren.
 * UI-återkoppling: UI:t måste tydligt visa vilka resultat som är sparade lokalt och väntar på synkronisering (t.ex. med en liten "moln"-ikon).
"Online"-läge och synkronisering
 * Detektering: När anslutningen återupprättas, ska en synkroniseringsprocess automatiskt triggas.
 * Hämta data: Processen anropar CacheStorageService.GetAllAsync för att hämta alla väntande resultat från cachen.
 * Batch-sändning: Resultaten skickas i en batch till en dedikerad API-slutpunkt på servern (t.ex. POST /api/sync-results).
 * Serverbearbetning: Servern tar emot batchen, validerar datan och sparar den till databasen inom en transaktion för att säkerställa atomicitet.
 * Bekräftelse och rensning: Om servern framgångsrikt bearbetar batchen, returnerar den en bekräftelse. Klienten tar då emot denna bekräftelse och anropar CacheStorageService.RemoveAsync för varje synkroniserat objekt för att rensa cachen.
Denna synkroniseringslogik är den mest felbenägna delen av systemet. Den måste vara idempotent, vilket innebär att om samma batch av data skickas två gånger (t.ex. på grund av ett nätverksfel efter sändning men före bekräftelse), får det inte leda till dubbletter i databasen. Detta kan hanteras genom att varje InspectionResult har ett unikt ID (GUID) som genereras på klienten.
5.4 Konflikthantering
Att ha två källor till sanning (serverdatabasen och klientcachen) introducerar risken för datakonflikter. Till exempel: en kontrollant är offline och markerar objekt X som "OK", men under tiden har en TenantAdmin raderat objekt X från översiktsritningen.
Initial strategi
För den första versionen av systemet implementeras en enkel "client-wins-with-logging"-strategi:
 * Om servern tar emot ett resultat för ett objekt som inte längre existerar, ska resultatet ignoreras, men en detaljerad loggpost ska skapas. En avisering kan skickas till TenantAdmin för att informera om konflikten.
 * För uppdateringar av samma objekt vinner den senaste skrivningen.
För framtida versioner kan mer avancerade strategier övervägas, såsom att använda versionsnummer eller tidsstämplar (rowversion/timestamp) för optimistisk låsning.

