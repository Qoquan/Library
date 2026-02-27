# 📚 Library — Guide Complet du Projet POO

> **UE :** Programmation Orientée Objet | **Code :** 5IOBJ-1  
> **Stack :** C# .NET 8 · Blazor Server · ASP.NET Core API · Entity Framework Core · SQLite · OpenLibrary API

---

## 📋 Table des matières

1. [Vue d'ensemble du projet](#1-vue-densemble-du-projet)
2. [Architecture et structure des fichiers](#2-architecture-et-structure-des-fichiers)
3. [Prérequis — Ce qu'il faut installer](#3-prérequis)
4. [Installation pas à pas dans VS Code](#4-installation-pas-à-pas)
5. [Lancer le projet](#5-lancer-le-projet)
6. [Les concepts POO utilisés](#6-concepts-poo)
7. [L'API interne (CRUD local)](#7-api-interne)
8. [L'API externe (OpenLibrary)](#8-api-externe-openlibrary)
9. [Les tests unitaires (AA2)](#9-tests-unitaires)
10. [Justification des choix (AA3)](#10-justification-des-choix)
11. [Grille d'évaluation — correspondance](#11-grille-dévaluation)

---

## 1. Vue d'ensemble du projet

### Qu'est-ce que ce projet ?

**Library** est une application de gestion de bibliothèque complète construite avec trois projets C# qui communiquent entre eux :

```
[ Navigateur Web ]
       ↓ affiche
[ Library.Web — Blazor Server ]     ← Interface graphique
       ↓ appelle via HTTP
[ Library.API — ASP.NET Core ]      ← API REST interne
       ↓ lit/écrit              ↓ appelle via HTTP
[ SQLite (library.db) ]    [ OpenLibrary.org API externe ]
```

### Ce que l'application permet de faire

| Fonctionnalité | Description |
|---|---|
| ✅ **Lister** les livres | Voir tous les livres avec leur disponibilité |
| ✅ **Ajouter** un livre | Formulaire de création avec validation |
| ✅ **Modifier** un livre | Édition de toutes les informations |
| ✅ **Supprimer** un livre | Avec confirmation modale |
| ✅ **Rechercher** en local | Par titre, auteur, genre, ISBN |
| ✅ **Emprunter/Retourner** | Changer la disponibilité d'un livre |
| ✅ **Rechercher en ligne** | Via l'API OpenLibrary (millions de livres) |
| ✅ **Importer** un livre | Depuis OpenLibrary vers la base locale |

---

## 2. Architecture et structure des fichiers

### Arborescence complète

```
Library/
│
├── Library.sln                          ← Fichier solution (ouvre tout le projet)
│
├── Library.Shared/                      ── COUCHE MODÈLES (partagée)
│   ├── Library.Shared.csproj
│   └── Models/
│       ├── Book.cs                      ← Classe Book (modèle principal)
│       └── OpenLibraryBook.cs           ← DTOs pour l'API externe
│
├── Library.API/                         ── COUCHE API (backend)
│   ├── Library.API.csproj
│   ├── Program.cs                       ← Configuration & démarrage
│   ├── appsettings.json                 ← Config (connexion BD, ports)
│   ├── Controllers/
│   │   ├── BooksController.cs           ← Endpoints CRUD /api/books
│   │   └── OpenLibraryController.cs     ← Endpoints /api/openlibrary
│   ├── Data/
│   │   └── LibraryDbContext.cs          ← Contexte Entity Framework
│   ├── Services/
│   │   ├── IBookService.cs              ← Interface (contrat)
│   │   ├── BookService.cs               ← Implémentation CRUD
│   │   └── OpenLibraryService.cs        ← Appels API externe
│   └── Tests/
│       └── BookServiceTests.cs          ← 10 tests unitaires (xUnit)
│
└── Library.Web/                         ── COUCHE UI (frontend Blazor)
    ├── Library.Web.csproj
    ├── Program.cs                       ← Config Blazor + HttpClient
    ├── appsettings.json                 ← URL de l'API
    ├── App.razor                        ← Composant racine + routeur
    ├── Pages/
    │   ├── _Host.cshtml                 ← Page HTML hôte
    │   ├── Index.razor                  ← Accueil + statistiques
    │   ├── Books.razor                  ← Liste + CRUD complet
    │   ├── BookForm.razor               ← Formulaire add/edit
    │   └── SearchExternal.razor         ← Recherche OpenLibrary
    ├── Services/
    │   └── BookApiService.cs            ← Appels HTTP vers l'API
    ├── Shared/
    │   └── MainLayout.razor             ← Navbar + layout
    └── wwwroot/
        └── css/
            └── app.css                  ← Styles personnalisés
```

### Pourquoi 3 projets séparés ?

| Projet | Rôle | Pourquoi séparé ? |
|---|---|---|
| `Library.Shared` | Modèles (classes) | Évite la duplication : Book.cs est utilisé dans l'API **et** dans Blazor |
| `Library.API` | Logique métier + BD | Indépendant de l'UI, testable, remplaçable |
| `Library.Web` | Interface utilisateur | Peut être remplacé par une app mobile sans toucher l'API |

---

## 3. Prérequis

### Logiciels à installer

#### 1. .NET 8 SDK
```
https://dotnet.microsoft.com/download/dotnet/8.0
```
Vérification après installation :
```bash
dotnet --version
# Doit afficher : 8.x.x
```

#### 2. Visual Studio Code
```
https://code.visualstudio.com/
```

#### 3. Extensions VS Code à installer

Ouvre VS Code → `Ctrl+Shift+X` → cherche et installe :

| Extension | Pourquoi |
|---|---|
| **C# Dev Kit** (Microsoft) | Support C#, IntelliSense, débogage |
| **C# Extensions** (JosKreativ) | Génération de classes, snippets |
| **.NET Install Tool** (Microsoft) | Gestion automatique du SDK |

#### 4. Vérifier que tout fonctionne
```bash
dotnet --version          # 8.x.x
dotnet tool list -g       # outils globaux
```

---

## 4. Installation pas à pas

### Étape 1 — Récupérer les fichiers

Place tous les fichiers dans la structure suivante (respecte exactement les chemins) :

```
C:\Users\TON_NOM\Library\     (Windows)
~/Library/                     (Mac/Linux)
```

### Étape 2 — Ouvrir dans VS Code

```bash
# Depuis le terminal, aller dans le dossier Library
cd Library

# Ouvrir VS Code
code .
```

Ou : `Fichier → Ouvrir le dossier → sélectionne le dossier Library`

### Étape 3 — Restaurer les packages NuGet

Dans le terminal intégré de VS Code (`Ctrl + J`) :

```bash
# Se placer à la racine du projet
cd Library

# Restaurer tous les packages des 3 projets
dotnet restore
```

> 💡 Cette commande télécharge automatiquement : Entity Framework Core, SQLite, Swashbuckle (Swagger)

### Étape 4 — Créer la base de données

```bash
# Aller dans le projet API
cd Library.API

# Créer la base SQLite et appliquer les données initiales
dotnet ef database update
```

> ℹ️ Si `dotnet ef` n'est pas trouvé :
> ```bash
> dotnet tool install --global dotnet-ef
> ```

> ℹ️ Si EF Tools n'est pas installé dans le projet :
> ```bash
> dotnet add package Microsoft.EntityFrameworkCore.Tools
> ```

**Alternative** : La base est aussi créée automatiquement au premier démarrage grâce à `db.Database.EnsureCreated()` dans `Program.cs`.

### Étape 5 — Installer les packages du projet Tests (optionnel)

```bash
# Créer le projet de tests séparé
dotnet new xunit -n Library.Tests
cd Library.Tests
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add reference ../Library.API/Library.API.csproj

# Copier le fichier BookServiceTests.cs dans Library.Tests/
# Puis lancer les tests
dotnet test
```

---

## 5. Lancer le projet

### Méthode recommandée : 2 terminaux simultanés

#### Terminal 1 — Démarrer l'API (backend)

```bash
cd Library.API
dotnet run
```

Résultat attendu :
```
=== Library API démarrée ===
Swagger : http://localhost:5000/swagger
API     : http://localhost:5000/api/books
info: Now listening on: http://localhost:5000
```

#### Terminal 2 — Démarrer Blazor (frontend)

```bash
cd Library.Web
dotnet run
```

Résultat attendu :
```
=== Library Web démarrée ===
Connexion API : http://localhost:5000
info: Now listening on: http://localhost:5001
```

### Accéder à l'application

| URL | Description |
|---|---|
| `http://localhost:5001` | Interface Blazor (application principale) |
| `http://localhost:5000/swagger` | Documentation interactive de l'API |
| `http://localhost:5000/api/books` | Données JSON brutes |

### Tester l'API directement (via Swagger)

1. Ouvre `http://localhost:5000/swagger`
2. Clique sur `GET /api/books` → **Try it out** → **Execute**
3. Tu verras les 3 livres de démonstration en JSON

---

## 6. Concepts POO

Ce projet illustre **tous les piliers de la programmation orientée objet** en C#.

### 6.1 Encapsulation

```csharp
// Book.cs — les propriétés encapsulent les données
public class Book
{
    public int Id { get; set; }           // lecture + écriture
    public string Title { get; set; }     // avec validation
    public bool IsAvailable { get; set; } // état interne protégé
}
```
Les données sont accessibles uniquement via les propriétés, jamais directement.

### 6.2 Héritage

```csharp
// BooksController hérite de ControllerBase (ASP.NET Core)
public class BooksController : ControllerBase
{
    // Hérite de : Ok(), NotFound(), BadRequest(), CreatedAtAction()...
}

// LibraryDbContext hérite de DbContext (Entity Framework)
public class LibraryDbContext : DbContext
{
    // Hérite de : SaveChanges(), Set<T>(), OnModelCreating()...
}
```

### 6.3 Abstraction (Interface)

```csharp
// IBookService.cs — définit le CONTRAT (ce que le service DOIT faire)
public interface IBookService
{
    Task<IEnumerable<Book>> GetAllAsync();
    Task<Book> CreateAsync(Book book);
    Task<bool> DeleteAsync(int id);
    // ...
}

// BookService.cs — IMPLÉMENTE le contrat
public class BookService : IBookService
{
    public async Task<Book> CreateAsync(Book book) { /* code */ }
}
```

**Avantage :** Le contrôleur dépend de l'*interface*, pas de l'implémentation. On peut remplacer SQLite par PostgreSQL sans changer le contrôleur.

### 6.4 Injection de dépendances

```csharp
// Program.cs — enregistrement
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddHttpClient<IOpenLibraryService, OpenLibraryService>();

// BooksController.cs — injection dans le constructeur
public BooksController(IBookService bookService, ILogger<BooksController> logger)
{
    _bookService = bookService;  // injecté automatiquement
    _logger = logger;
}
```

### 6.5 Polymorphisme

```csharp
// OpenLibraryDoc.ToBook() — conversion polymorphique
public Book ToBook()
{
    return new Book
    {
        Title = this.Title,
        Author = this.AuthorName?.FirstOrDefault() ?? "Inconnu",
        Source = "openlibrary"
    };
}
// Le même objet Book est utilisé qu'il vienne de SQLite ou d'OpenLibrary
```

### 6.6 Responsabilité unique (SOLID — S)

| Classe | Unique responsabilité |
|---|---|
| `Book` | Représenter les données d'un livre |
| `BookService` | Opérations CRUD sur la base de données |
| `OpenLibraryService` | Appels HTTP vers l'API externe |
| `BooksController` | Recevoir les requêtes HTTP et retourner les réponses |
| `BookApiService` (Web) | Appels HTTP depuis Blazor vers l'API interne |

---

## 7. API interne

### Endpoints disponibles

| Méthode | URL | Description |
|---|---|---|
| `GET` | `/api/books` | Liste tous les livres |
| `GET` | `/api/books/{id}` | Récupère un livre par ID |
| `GET` | `/api/books/search?q=harry` | Recherche par mot-clé |
| `POST` | `/api/books` | Crée un nouveau livre |
| `PUT` | `/api/books/{id}` | Met à jour un livre |
| `DELETE` | `/api/books/{id}` | Supprime un livre |
| `PATCH` | `/api/books/{id}/toggle` | Inverse la disponibilité |
| `GET` | `/api/openlibrary/search?q=dune` | Recherche sur OpenLibrary |
| `POST` | `/api/openlibrary/import` | Importe un livre dans SQLite |

### Exemple de réponse JSON

```json
GET /api/books/1

{
  "id": 1,
  "title": "Le Petit Prince",
  "author": "Antoine de Saint-Exupéry",
  "isbn": "978-2-07-040850-4",
  "publishedYear": 1943,
  "genre": "Conte philosophique",
  "description": "Un pilote rencontre un mystérieux petit garçon...",
  "coverUrl": null,
  "isAvailable": true,
  "createdAt": "2024-01-01T00:00:00",
  "source": "local"
}
```

### Flux d'une requête (de l'UI à la BD)

```
Blazor (Books.razor)
    ↓ appelle BookApiService.CreateBookAsync(book)
BookApiService
    ↓ POST http://localhost:5000/api/books (JSON)
BooksController.Create()
    ↓ valide ModelState
    ↓ appelle _bookService.CreateAsync(book)
BookService.CreateAsync()
    ↓ _context.Books.Add(book)
    ↓ _context.SaveChangesAsync()
SQLite (library.db)
    ↑ retourne le livre avec son nouvel ID
BooksController
    ↑ retourne 201 Created + JSON
BookApiService
    ↑ désérialise le JSON → Book
Books.razor
    ↑ affiche le résultat à l'utilisateur
```

---

## 8. API externe OpenLibrary

### Pourquoi OpenLibrary ?

- **Gratuite** et sans clé API requise
- **Open source** (projet Internet Archive)
- Contient **plus de 20 millions** de livres
- Documentation : `https://openlibrary.org/developers/api`

### Comment ça fonctionne

```
Utilisateur tape "Harry Potter" dans SearchExternal.razor
    ↓
BookApiService.SearchOpenLibraryAsync("Harry Potter")
    ↓ GET /api/openlibrary/search?q=Harry+Potter
OpenLibraryController.Search()
    ↓ appelle OpenLibraryService
OpenLibraryService.SearchBooksAsync()
    ↓ GET https://openlibrary.org/search.json?q=Harry+Potter&limit=10
OpenLibrary.org répond avec du JSON
    ↓
OpenLibraryService désérialise → List<OpenLibraryDoc>
    ↓ .ToBook() sur chaque élément
    ↓ retourne List<Book>
SearchExternal.razor affiche les résultats
    ↓ utilisateur clique "Importer"
POST /api/openlibrary/import → sauvegarde dans SQLite
```

### Structure JSON de l'API OpenLibrary

```json
{
  "numFound": 512,
  "docs": [
    {
      "title": "Harry Potter and the Philosopher's Stone",
      "author_name": ["J. K. Rowling"],
      "isbn": ["9780439708180"],
      "first_publish_year": 1997,
      "subject": ["Magic", "Wizards"],
      "cover_i": 8228691
    }
  ]
}
```

La couverture est construite ainsi :
```
https://covers.openlibrary.org/b/id/8228691-M.jpg
```

---

## 9. Tests unitaires

### Fichier : `Library.API/Tests/BookServiceTests.cs`

Le fichier contient **10 scénarios de test** couvrant tous les cas d'usage :

| # | Scénario | Ce qui est testé |
|---|---|---|
| 1 | Base vide → liste vide | `GetAllAsync` |
| 2 | Créer → livre avec ID assigné | `CreateAsync` |
| 3 | GetById inconnu → null | `GetByIdAsync` |
| 4 | GetById existant → bon livre | `GetByIdAsync` |
| 5 | Update → données modifiées | `UpdateAsync` |
| 6 | Update inconnu → null | `UpdateAsync` |
| 7 | Delete → livre supprimé | `DeleteAsync` |
| 8 | Delete inconnu → false | `DeleteAsync` |
| 9 | Search par titre → filtre correct | `SearchAsync` |
| 10 | Toggle 2x → revient à l'état initial | `ToggleAvailabilityAsync` |

### Stratégie de test : Base InMemory

```csharp
// Chaque test utilise une base de données isolée en mémoire
var options = new DbContextOptionsBuilder<LibraryDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;
```

**Avantage :** Les tests sont rapides (pas de SQLite), isolés (pas d'interférences), et répétables.

### Lancer les tests

```bash
# Depuis le dossier Library.Tests
dotnet test

# Avec détails
dotnet test --verbosity normal

# Résultat attendu :
# Passed! - Failed: 0, Passed: 10, Skipped: 0, Total: 10
```

---

## 10. Justification des choix

### AA3 — Choix architecturaux

#### Pourquoi Blazor Server ?

| Critère | Blazor Server | Blazor WASM |
|---|---|---|
| **Démarrage** | ✅ Instantané | ❌ Télécharge le runtime .NET |
| **Accès BD** | ✅ Direct (même serveur) | ❌ Impossible sans API |
| **Projet scolaire** | ✅ Plus simple | ❌ Config CORS complexe |
| **SEO** | ✅ HTML rendu côté serveur | ❌ Nécessite prerendering |
| **Offline** | ❌ Nécessite connexion | ✅ Fonctionne hors ligne |

→ **Blazor Server** est le meilleur choix pour un projet académique.

#### Pourquoi SQLite ?

- **Zéro configuration** : un simple fichier `.db`
- **Idéal en développement** : pas de serveur à installer
- **Compatible EF Core** : migration facile vers PostgreSQL/SQL Server en production
- **Portable** : le fichier `library.db` se déplace avec l'application

#### Pourquoi Entity Framework Core ?

- **Abstraction** de la base de données (pas de SQL manuel)
- **Type-safe** : les requêtes sont vérifiées à la compilation
- **Migration** : évolution du schéma sans perdre les données
- **LINQ** : requêtes en C# natif (`Where`, `OrderBy`, etc.)

#### Pourquoi l'interface IBookService ?

```csharp
// Sans interface — couplage fort (BAD)
public class BooksController {
    private BookService _service; // dépend de la classe concrète
}

// Avec interface — couplage faible (GOOD)
public class BooksController {
    private IBookService _service; // dépend du contrat
    // → On peut passer MockBookService dans les tests
    // → On peut remplacer par PostgreSQLBookService sans toucher le contrôleur
}
```

#### Pourquoi OpenLibrary plutôt que Google Books ?

| Critère | OpenLibrary | Google Books API |
|---|---|---|
| **Clé API** | ✅ Aucune requise | ❌ Compte Google + clé |
| **Quota** | ✅ Illimité | ❌ 1000 req/jour gratuit |
| **Open source** | ✅ Oui | ❌ Propriétaire |
| **Richesse** | ✅ 20M+ livres | ✅ 40M+ livres |
| **Facilité** | ✅ Simple | ❌ OAuth possible |

→ **OpenLibrary** est idéal pour un projet scolaire : aucune inscription, aucune clé.

### Méthode de résolution (AA3)

Le projet suit l'approche **Domain-Driven Design simplifié** :

1. **Modélisation** → Identifier l'entité centrale : `Book`
2. **Couche données** → `LibraryDbContext` + `BookService`
3. **Couche API** → Contrôleurs REST exposant les opérations
4. **Couche UI** → Pages Blazor consommant l'API
5. **Tests** → Validation de chaque opération métier

---

## 11. Grille d'évaluation

### AA1 — Concevoir, installer et utiliser des objets appropriés `/25`

| Critère | Implémentation dans le projet |
|---|---|
| **Concevoir une application qui appelle une API et gère des données locales** | `OpenLibraryService.cs` appelle `https://openlibrary.org` · `BookService.cs` gère SQLite local via EF Core |
| **Installer les composants nécessaires** | 3 packages NuGet dans `.csproj` · `dotnet restore` · `EnsureCreated()` pour la BD |
| **Utiliser les objets appropriés** | `Book`, `HttpClient`, `DbContext`, `IBookService`, `BookApiService`, contrôleurs, services injectés |

### AA2 — Concevoir et mettre en œuvre des tests `/10`

| Critère | Implémentation |
|---|---|
| **Définir les scénarios de test** | 10 scénarios documentés couvrant CRUD + cas limites (ID inexistant, liste vide...) |
| **Implémenter des tests unitaires** | `BookServiceTests.cs` · xUnit · EF InMemory · arrange/act/assert |

### AA3 — Justifier les choix `/15`

| Critère | Où trouver la justification |
|---|---|
| **Justifier la méthode de résolution** | Section 10 de ce README · commentaires dans `Program.cs` |
| **Justifier les choix conceptuels** | Section 6 (POO) + Section 10 (Architecture) · comparatifs Blazor/SQLite/OpenLibrary |

### AA4 — Travail de groupe `/50`

Répartition suggérée des responsabilités :

| Membre | Module | Fichiers |
|---|---|---|
| Étudiant 1 | Modèles + Base de données | `Book.cs`, `LibraryDbContext.cs`, `BookService.cs` |
| Étudiant 2 | API REST | `BooksController.cs`, `OpenLibraryController.cs`, `OpenLibraryService.cs` |
| Étudiant 3 | Frontend Blazor | `Books.razor`, `BookForm.razor`, `SearchExternal.razor` |
| Étudiant 4 | Tests + Documentation | `BookServiceTests.cs`, ce README |

---

## 🚀 Commandes de démarrage rapide

```bash
# 1. Restaurer les packages
dotnet restore

# 2. Démarrer l'API (Terminal 1)
cd Library.API && dotnet run

# 3. Démarrer Blazor (Terminal 2)
cd Library.Web && dotnet run

# 4. Lancer les tests (Terminal 3)
cd Library.Tests && dotnet test

# 5. Accéder à l'application
# → http://localhost:5001          (interface Blazor)
# → http://localhost:5000/swagger  (documentation API)
```

---

## ❓ Problèmes fréquents

| Problème | Solution |
|---|---|
| `dotnet: command not found` | Installer .NET 8 SDK depuis microsoft.com/dotnet |
| `Unable to connect to API` | Vérifier que Library.API tourne sur le port 5000 |
| `No such file library.db` | La BD est créée automatiquement au 1er `dotnet run` |
| `CORS error` | Vérifier que l'URL du Web est bien dans la policy CORS de `Program.cs` |
| `dotnet ef not found` | `dotnet tool install --global dotnet-ef` |
| OpenLibrary retourne vide | Vérifier la connexion internet · L'API est parfois lente |

---

*Projet réalisé dans le cadre du cours de Programmation Orientée Objet — IFOSUP*