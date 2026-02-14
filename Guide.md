# 📚 Guide Complet - Bibliothèque Personnelle en C# avec Blazor WebAssembly

## Table des matières

1. [Architecture du projet](#architecture-du-projet)
2. [Couche Core (Domain)](#couche-core-domain)
3. [Couche Infrastructure](#couche-infrastructure)
4. [Couche API](#couche-api)
5. [Couche Client (Blazor)](#couche-client-blazor)
6. [Tests](#tests)
7. [Concepts POO utilisés](#concepts-poo-utilisés)
8. [Patterns de conception](#patterns-de-conception)

---

## Architecture du projet

### 🏗️ Architecture en Couches (Layered Architecture)

Le projet suit une **architecture en couches** (Clean Architecture / Onion Architecture) :

```
┌─────────────────────────────────────┐
│     BibliothequePersonnelle.Client  │  ← Interface utilisateur (Blazor WebAssembly)
│              (Presentation)         │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│      BibliothequePersonnelle.API    │  ← API REST (ASP.NET Core)
│           (Application)             │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│ BibliothequePersonnelle.Infrastructure │ ← Accès données + Services externes
│          (Infrastructure)           │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│      BibliothequePersonnelle.Core   │  ← Logique métier + Entités
│             (Domain)                │
└─────────────────────────────────────┘
```

### Avantages de cette architecture :

1. **Séparation des responsabilités** : Chaque couche a un rôle précis
2. **Testabilité** : Les couches peuvent être testées indépendamment
3. **Maintenabilité** : Modifications isolées sans impact sur les autres couches
4. **Indépendance technologique** : Le Core ne dépend d'aucune technologie spécifique

---

## Couche Core (Domain)

### 📦 Responsabilité
Contient la **logique métier** pure, sans dépendance vers des frameworks ou technologies externes.

### Fichiers principaux

#### 1. **Entities/Livre.cs** - L'entité de domaine

```csharp
public class Livre
{
    public int Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Auteur { get; set; } = string.Empty;
    // ... autres propriétés
}
```

**Explication :**
- **Entité** : Représente un objet métier avec une identité unique (Id)
- **`string.Empty`** : Initialisation par défaut pour éviter les valeurs null
- **`string?`** : Le `?` indique que la propriété peut être null (Nullable Reference Types de C# 8+)
- **`DateTime.UtcNow`** : UTC (Temps Universel Coordonné) pour éviter les problèmes de fuseaux horaires

**Concepts POO :**
- ✅ **Encapsulation** : Propriétés publiques avec getters/setters
- ✅ **Abstraction** : Représentation simplifiée d'un livre réel

---

#### 2. **DTOs (Data Transfer Objects)**

```csharp
public class LivreDto
{
    // Utilisé pour transférer des données du serveur vers le client
}

public class CreateLivreDto
{
    // Utilisé pour créer un nouveau livre
}

public class UpdateLivreDto
{
    // Utilisé pour mettre à jour un livre existant
}
```

**Pourquoi des DTOs ?**
- **Sécurité** : Ne pas exposer l'entité complète
- **Flexibilité** : Différentes représentations pour différentes opérations
- **Performance** : Transférer uniquement les données nécessaires

**Pattern utilisé :** **DTO Pattern** (Data Transfer Object)

---

#### 3. **Interfaces/ILivreRepository.cs** - Contrat du Repository

```csharp
public interface ILivreRepository
{
    Task<IEnumerable<Livre>> GetAllAsync();
    Task<Livre?> GetByIdAsync(int id);
    Task<Livre> CreateAsync(Livre livre);
    Task<Livre?> UpdateAsync(int id, Livre livre);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Livre>> SearchAsync(string searchTerm);
}
```

**Explication :**
- **Interface** : Définit un contrat sans implémentation
- **`Task<T>`** : Opération asynchrone retournant un type T
- **`async/await`** : Permet des opérations non-bloquantes (I/O, réseau, BD)
- **`?` après le type** : Indique que le résultat peut être null

**Concepts POO :**
- ✅ **Abstraction** : Définit "quoi" faire, pas "comment"
- ✅ **Polymorphisme** : Plusieurs implémentations possibles de cette interface
- ✅ **Inversion de dépendance** (SOLID) : Les couches supérieures dépendent d'abstractions

**Pattern utilisé :** **Repository Pattern** - Abstraction de l'accès aux données

---

#### 4. **Interfaces/IExternalBookService.cs** - Service externe

```csharp
public interface IExternalBookService
{
    Task<IEnumerable<ExternalBookDto>> SearchBooksAsync(string query);
    Task<ExternalBookDto?> GetBookByISBNAsync(string isbn);
}
```

**Explication :**
- Définit un contrat pour interagir avec une API externe (Google Books)
- Permet de changer facilement de fournisseur sans modifier le code métier

---

## Couche Infrastructure

### 🔧 Responsabilité
Implémente les **détails techniques** : accès à la base de données, appels API externes, etc.

### Fichiers principaux

#### 1. **Data/BibliothequeDbContext.cs** - Contexte Entity Framework

```csharp
public class BibliothequeDbContext : DbContext
{
    public BibliothequeDbContext(DbContextOptions<BibliothequeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Livre> Livres { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Livre>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titre).IsRequired().HasMaxLength(200);
            // ... configuration des colonnes
        });
    }
}
```

**Explication ligne par ligne :**

1. **`DbContext`** : Classe de base EF Core pour gérer les connexions BD
2. **Constructeur avec `DbContextOptions`** : Injection de dépendances pour la configuration
3. **`: base(options)`** : Appel au constructeur de la classe parent
4. **`DbSet<Livre>`** : Collection représentant une table en base de données
5. **`OnModelCreating`** : Configuration du mapping objet-relationnel (ORM)
6. **`HasKey`** : Définit la clé primaire
7. **`IsRequired`** : Colonne NOT NULL
8. **`HasMaxLength`** : Limite de caractères

**Concepts POO :**
- ✅ **Héritage** : BibliothequeDbContext hérite de DbContext
- ✅ **Encapsulation** : Masque les détails de la base de données

**Pattern utilisé :** **Unit of Work** (géré par DbContext d'EF Core)

---

#### 2. **Repositories/LivreRepository.cs** - Implémentation du Repository

```csharp
public class LivreRepository : ILivreRepository
{
    private readonly BibliothequeDbContext _context;

    public LivreRepository(BibliothequeDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Livre>> GetAllAsync()
    {
        return await _context.Livres
            .OrderByDescending(l => l.DateAjout)
            .ToListAsync();
    }

    // ... autres méthodes
}
```

**Explication :**

1. **`private readonly`** : Champ privé immuable (bonne pratique)
2. **Constructeur avec paramètre** : Injection de dépendances
3. **`async Task<T>`** : Méthode asynchrone
4. **`await`** : Attend le résultat d'une opération asynchrone
5. **LINQ** : `.OrderByDescending`, `.Where`, etc. (Language Integrated Query)
6. **`.ToListAsync()`** : Exécute la requête de façon asynchrone

**Concepts POO :**
- ✅ **Encapsulation** : `_context` est privé
- ✅ **Implémentation d'interface** : Réalise le contrat ILivreRepository
- ✅ **Injection de dépendances** : Reçoit ses dépendances via le constructeur

---

#### 3. **Services/GoogleBooksService.cs** - Appel API externe

```csharp
public class GoogleBooksService : IExternalBookService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://www.googleapis.com/books/v1/volumes";

    public GoogleBooksService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<ExternalBookDto>> SearchBooksAsync(string query)
    {
        var response = await _httpClient.GetStringAsync(
            $"{BaseUrl}?q={Uri.EscapeDataString(query)}&maxResults=10"
        );
        var result = JsonConvert.DeserializeObject<GoogleBooksResponse>(response);
        
        if (result?.Items == null)
            return Enumerable.Empty<ExternalBookDto>();

        return result.Items.Select(MapToExternalBookDto);
    }

    private ExternalBookDto MapToExternalBookDto(GoogleBookItem item)
    {
        // Mapping des données Google Books vers notre DTO
    }
}
```

**Explication :**

1. **`HttpClient`** : Client HTTP pour appeler des APIs REST
2. **`const`** : Constante (ne change jamais)
3. **`Uri.EscapeDataString`** : Encode l'URL pour éviter les caractères spéciaux
4. **`JsonConvert.DeserializeObject`** : Convertit JSON en objet C#
5. **Opérateur `?.`** : Null-conditional operator (évite les NullReferenceException)
6. **`??`** : Null-coalescing operator (valeur par défaut si null)
7. **LINQ `.Select()`** : Transforme chaque élément

**Pattern utilisé :** **Adapter Pattern** - Adapte l'API externe à notre interface

---

## Couche API

### 🌐 Responsabilité
Expose les fonctionnalités via une **API REST** (endpoints HTTP).

### Fichiers principaux

#### 1. **Controllers/LivresController.cs** - Contrôleur REST

```csharp
[ApiController]
[Route("api/[controller]")]
public class LivresController : ControllerBase
{
    private readonly ILivreRepository _repository;

    public LivresController(ILivreRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LivreDto>>> GetAll()
    {
        var livres = await _repository.GetAllAsync();
        var livresDto = livres.Select(MapToDto);
        return Ok(livresDto);
    }

    [HttpPost]
    public async Task<ActionResult<LivreDto>> Create(CreateLivreDto createDto)
    {
        var livre = new Livre
        {
            Titre = createDto.Titre,
            Auteur = createDto.Auteur,
            // ... mapping
        };

        var createdLivre = await _repository.CreateAsync(livre);
        return CreatedAtAction(nameof(GetById), new { id = createdLivre.Id }, MapToDto(createdLivre));
    }

    // ... autres endpoints
}
```

**Explication des attributs :**

- **`[ApiController]`** : Active les comportements spécifiques aux API (validation auto, etc.)
- **`[Route("api/[controller]")]`** : Définit l'URL de base (`/api/livres`)
- **`[HttpGet]`** : Endpoint pour les requêtes GET
- **`[HttpPost]`** : Endpoint pour les requêtes POST
- **`[HttpPut("{id}")]`** : Endpoint PUT avec paramètre de route
- **`[HttpDelete("{id}")]`** : Endpoint DELETE

**Codes de statut HTTP :**
- **`Ok()`** : 200 - Succès
- **`CreatedAtAction()`** : 201 - Ressource créée
- **`NotFound()`** : 404 - Ressource non trouvée
- **`NoContent()`** : 204 - Succès sans contenu
- **`BadRequest()`** : 400 - Requête invalide

**Pattern utilisé :** **MVC Pattern** (Model-View-Controller), ici sans View

---

#### 2. **Program.cs** - Configuration de l'application

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configuration des services (Dependency Injection Container)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration de la base de données
builder.Services.AddDbContext<BibliothequeDbContext>(options =>
    options.UseSqlite("Data Source=bibliotheque.db"));

// Injection de dépendances
builder.Services.AddScoped<ILivreRepository, LivreRepository>();
builder.Services.AddHttpClient<IExternalBookService, GoogleBooksService>();

// Configuration CORS pour Blazor WebAssembly
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy =>
        {
            policy.WithOrigins("https://localhost:7001", "http://localhost:5001")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Créer la base de données au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BibliothequeDbContext>();
    db.Database.EnsureCreated();
}

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowBlazorClient");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Explication :**

1. **`WebApplication.CreateBuilder`** : Crée le builder de l'application
2. **`AddControllers`** : Active les contrôleurs MVC
3. **`AddDbContext`** : Enregistre le DbContext dans le conteneur DI
4. **`AddScoped`** : Crée une instance par requête HTTP
5. **`AddHttpClient`** : Configure un HttpClient avec injection de dépendances
6. **CORS** : Cross-Origin Resource Sharing - permet aux clients web d'appeler l'API
7. **`EnsureCreated`** : Crée la BD si elle n'existe pas
8. **Pipeline middleware** : Chaîne de responsabilités pour traiter les requêtes HTTP

**Pattern utilisé :** **Dependency Injection** (Inversion of Control)

---

## Couche Client (Blazor)

### 💻 Responsabilité
Interface utilisateur **Single Page Application** (SPA) en Blazor WebAssembly.

### Fichiers principaux

#### 1. **Services/LivreService.cs** - Service client pour appeler l'API

```csharp
public class LivreService : ILivreService
{
    private readonly HttpClient _httpClient;
    private const string ApiUrl = "api/livres";

    public LivreService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<LivreDto>> GetAllAsync()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<LivreDto>>(ApiUrl) 
               ?? new List<LivreDto>();
    }

    public async Task<LivreDto> CreateAsync(CreateLivreDto createDto)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiUrl, createDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LivreDto>() 
               ?? throw new Exception("Failed to create livre");
    }

    // ... autres méthodes
}
```

**Explication :**

1. **`GetFromJsonAsync`** : GET + désérialisation JSON automatique
2. **`PostAsJsonAsync`** : POST + sérialisation JSON automatique
3. **`EnsureSuccessStatusCode`** : Lance une exception si code HTTP d'erreur
4. **`?? new List<>()`** : Retourne liste vide si null

---

#### 2. **Pages/Livres.razor** - Composant Blazor (page principale)

```razor
@page "/"
@page "/livres"
@using BibliothequePersonnelle.Client.Services
@using BibliothequePersonnelle.Core.DTOs
@inject ILivreService LivreService
@inject NavigationManager Navigation

<PageTitle>Ma Bibliothèque</PageTitle>

<div class="container mt-4">
    <h1>📚 Ma Bibliothèque Personnelle</h1>
    
    @if (isLoading)
    {
        <div class="spinner-border"></div>
    }
    else
    {
        @foreach (var livre in livres)
        {
            <div @onclick="() => NavigateToDetail(livre.Id)">
                <h5>@livre.Titre</h5>
                <p>@livre.Auteur</p>
            </div>
        }
    }
</div>

@code {
    private List<LivreDto> livres = new();
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadLivres();
    }

    private async Task LoadLivres()
    {
        isLoading = true;
        var result = await LivreService.GetAllAsync();
        livres = result.ToList();
        isLoading = false;
    }

    private void NavigateToDetail(int id)
    {
        Navigation.NavigateTo($"/livres/{id}");
    }
}
```

**Explication de la syntaxe Razor :**

1. **`@page "/"`** : Définit la route du composant
2. **`@using`** : Importe un namespace
3. **`@inject`** : Injection de dépendances dans le composant
4. **`@if`** : Condition côté serveur
5. **`@foreach`** : Boucle côté serveur
6. **`@livre.Titre`** : Affiche une propriété
7. **`@code { }`** : Bloc de code C#
8. **`protected override`** : Surcharge de méthode du cycle de vie
9. **`OnInitializedAsync`** : Appelé lors de l'initialisation du composant

**Cycle de vie d'un composant Blazor :**
1. **SetParametersAsync** : Paramètres reçus
2. **OnInitialized / OnInitializedAsync** : Initialisation
3. **OnParametersSet / OnParametersSetAsync** : Après chaque changement de paramètres
4. **OnAfterRender / OnAfterRenderAsync** : Après chaque rendu

---

#### 3. **Program.cs** (Client) - Configuration Blazor

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configuration du HttpClient avec l'URL de l'API
builder.Services.AddScoped(sp => 
    new HttpClient { BaseAddress = new Uri("https://localhost:7000") }
);

// Enregistrement des services
builder.Services.AddScoped<ILivreService, LivreService>();
builder.Services.AddScoped<IExternalBookClientService, ExternalBookClientService>();

await builder.Build().RunAsync();
```

**Explication :**

1. **`WebAssemblyHostBuilder`** : Builder pour Blazor WebAssembly
2. **`RootComponents`** : Composants racine de l'application
3. **`#app`** : Sélecteur CSS de l'élément DOM
4. **`BaseAddress`** : URL de base pour tous les appels HTTP
5. **`AddScoped`** : Dans Blazor WASM, équivaut à Singleton (pas de requêtes HTTP côté serveur)

---

## Tests

### 🧪 Types de tests

#### 1. **Tests Unitaires** - Testent une unité isolée

```csharp
public class LivreRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllLivres()
    {
        // Arrange (Préparation)
        var options = new DbContextOptionsBuilder<BibliothequeDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        
        var context = new BibliothequeDbContext(options);
        context.Livres.Add(new Livre { Titre = "Test", Auteur = "Author" });
        await context.SaveChangesAsync();
        
        var repository = new LivreRepository(context);

        // Act (Action)
        var result = await repository.GetAllAsync();

        // Assert (Vérification)
        Assert.Single(result);
        Assert.Equal("Test", result.First().Titre);
    }
}
```

**Explication :**

1. **`[Fact]`** : Attribut xUnit pour marquer un test
2. **AAA Pattern** : Arrange, Act, Assert
3. **`UseInMemoryDatabase`** : Base de données en mémoire pour les tests
4. **`Assert`** : Vérifications du framework de test

---

#### 2. **Tests d'Intégration** - Testent plusieurs composants ensemble

```csharp
public class LivresControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LivresControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/livres");

        // Assert
        response.EnsureSuccessStatusCode();
        var livres = await response.Content.ReadFromJsonAsync<List<LivreDto>>();
        Assert.NotNull(livres);
    }
}
```

**Explication :**

1. **`IClassFixture`** : Partage une instance entre tests
2. **`WebApplicationFactory`** : Crée un serveur de test en mémoire
3. **Test end-to-end** : Teste tout le pipeline (Controller → Service → Repository → BD)

---

## Concepts POO utilisés

### 1. **Encapsulation**
```csharp
private readonly BibliothequeDbContext _context; // Champ privé
public async Task<Livre> CreateAsync(Livre livre) // Méthode publique
```
- Masque les détails d'implémentation
- Expose uniquement ce qui est nécessaire

### 2. **Abstraction**
```csharp
public interface ILivreRepository { } // Contrat abstrait
```
- Définit "quoi" sans "comment"
- Permet de changer l'implémentation facilement

### 3. **Héritage**
```csharp
public class BibliothequeDbContext : DbContext { }
public class LivresController : ControllerBase { }
```
- Réutilise le code de la classe parent
- Relation "est-un"

### 4. **Polymorphisme**
```csharp
ILivreRepository repo = new LivreRepository(context);
// Plusieurs implémentations possibles de ILivreRepository
```
- Même interface, comportements différents

### 5. **Composition**
```csharp
public class LivreRepository
{
    private readonly BibliothequeDbContext _context; // HAS-A relationship
}
```
- Relation "a-un"
- Favoriser la composition sur l'héritage

---

## Patterns de conception

### 1. **Repository Pattern**
- Abstraction de l'accès aux données
- `ILivreRepository` / `LivreRepository`

### 2. **Dependency Injection (DI)**
- Inversion of Control (IoC)
- Les dépendances sont injectées via constructeurs

### 3. **DTO Pattern**
- Séparation Entité ↔ Données transférées
- `Livre` (entité) vs `LivreDto`, `CreateLivreDto`

### 4. **Unit of Work**
- Géré par `DbContext` d'Entity Framework
- Transactions atomiques

### 5. **Adapter Pattern**
- `GoogleBooksService` adapte l'API externe

### 6. **MVC Pattern**
- Model (Entités/DTOs)
- View (Pages Blazor)
- Controller (Contrôleurs API)

---

## Principes SOLID

### **S** - Single Responsibility Principle
Chaque classe a une seule responsabilité :
- `LivreRepository` : Accès données uniquement
- `LivresController` : Gestion des requêtes HTTP uniquement

### **O** - Open/Closed Principle
Ouvert à l'extension, fermé à la modification :
- Ajouter une nouvelle implémentation de `ILivreRepository` sans modifier le code existant

### **L** - Liskov Substitution Principle
Toute implémentation de `ILivreRepository` peut remplacer une autre

### **I** - Interface Segregation Principle
Interfaces ciblées et spécifiques (pas de "god interface")

### **D** - Dependency Inversion Principle
Les couches supérieures dépendent d'abstractions, pas d'implémentations :
- `LivresController` dépend de `ILivreRepository`, pas de `LivreRepository`

---

## Asynchronisme en C#

### Pourquoi async/await ?

```csharp
// ❌ SYNCHRONE (bloquant)
public List<Livre> GetAll()
{
    return context.Livres.ToList(); // Bloque le thread pendant la requête BD
}

// ✅ ASYNCHRONE (non-bloquant)
public async Task<List<Livre>> GetAllAsync()
{
    return await context.Livres.ToListAsync(); // Libère le thread pendant l'I/O
}
```

**Avantages :**
- **Performance** : Le thread peut traiter d'autres requêtes pendant l'attente I/O
- **Scalabilité** : Plus de requêtes simultanées
- **Responsivité UI** : L'interface ne se bloque pas

---

## Nuances importantes

### Nullable Reference Types (C# 8+)

```csharp
string titre;        // ⚠️ Ne peut pas être null (warning si non initialisé)
string? isbn;        // ✅ Peut être null
```

### Opérateurs null

```csharp
livre?.Titre         // Null-conditional: retourne null si livre est null
livre ?? defaultLivre // Null-coalescing: retourne defaultLivre si livre est null
```

### LINQ (Language Integrated Query)

```csharp
var result = livres
    .Where(l => l.EstLu)              // Filtrer
    .OrderBy(l => l.Titre)            // Trier
    .Select(l => new LivreDto { })    // Projeter/Mapper
    .ToList();                        // Exécuter
```

---

## Résumé des commandes dotnet

```bash
# Créer un projet
dotnet new webapi -n MonProjet

# Ajouter un package NuGet
dotnet add package NomDuPackage

# Ajouter une référence de projet
dotnet add reference ../AutreProjet/AutreProjet.csproj

# Restaurer les dépendances
dotnet restore

# Compiler
dotnet build

# Exécuter
dotnet run

# Exécuter les tests
dotnet test

# Nettoyer
dotnet clean
```

---

## Conclusion

Ce projet démontre :
- ✅ **Architecture en couches** pour la maintenabilité
- ✅ **POO complète** : Encapsulation, Abstraction, Héritage, Polymorphisme
- ✅ **Patterns** : Repository, DI, DTO, MVC, Adapter
- ✅ **SOLID principles**
- ✅ **Tests** : Unitaires et d'intégration
- ✅ **Async/await** pour la performance
- ✅ **API REST** + **Blazor WebAssembly**

**Prochaines améliorations possibles :**
- Authentification/Autorisation (Identity)
- Pagination des résultats
- Upload d'images
- Cache (Redis)
- Logging (Serilog)
- Validation fluide (FluentValidation)
