# 🧪 Plan de Tests — Projet Library

> AA2 — Concevoir et mettre en œuvre une procédure de test partiel et intégré

---

## Vue d'ensemble

| Projet | Fichier | Type | Nb tests |
|---|---|---|---|
| `Library.Tests` | `Unit/BookServiceTests.cs` | Unitaire | 18 |
| `Library.Tests` | `Unit/OpenLibraryServiceTests.cs` | Unitaire | 7 |
| `Library.Tests` | `Unit/BookModelValidationTests.cs` | Unitaire | 7 |
| `Library.Tests` | `Integration/BooksControllerIntegrationTests.cs` | Intégration | 15 |
| `Library.Tests` | `Integration/OpenLibraryControllerIntegrationTests.cs` | Intégration | 5 |
| **TOTAL** | | | **52 tests** |

---

## Différence : Tests Unitaires vs Tests d'Intégration

```
Tests Unitaires                   Tests d'Intégration
─────────────────                 ────────────────────────────────
Testent UNE seule classe          Testent PLUSIEURS couches ensemble

BookService                       HTTP Request
    ↓ isolé                           ↓
[InMemory DB]                     Middleware ASP.NET Core
                                      ↓
Rapides (< 1ms/test)              BooksController
Pas de réseau                         ↓
Pas de dépendances externes       BookService
                                      ↓
Outils : xUnit + EF InMemory      EF Core InMemory DB

Trouvent : bugs de logique        Trouvent : bugs d'intégration,
           calculs incorrects               routing, sérialisation JSON,
           cas limites                      validation HTTP
```

---

## Scénarios de test détaillés

### 📦 BookService — Tests Unitaires (18 tests)

#### GetAllAsync (3 tests)

| # | Scénario | Condition | Résultat attendu |
|---|---|---|---|
| 1 | Récupérer tous les livres | 4 livres en base | Liste de 4 éléments |
| 2 | Ordre alphabétique | Base seedée | Livres triés par titre A→Z |
| 3 | Base vide | Aucun livre | Liste vide (pas d'exception) |

#### GetByIdAsync (3 tests)

| # | Scénario | Condition | Résultat attendu |
|---|---|---|---|
| 4 | Récupérer livre existant | ID = 1 | Book avec Id=1, titre correct |
| 5 | Récupérer livre inexistant | ID = 9999 | null |
| 6 | ID négatif | ID = -1 | null |

#### SearchAsync (6 tests)

| # | Scénario | Condition | Résultat attendu |
|---|---|---|---|
| 7 | Recherche par titre | "petit prince" (minuscules) | 1 résultat, insensible casse |
| 8 | Recherche par auteur | "orwell" | 1 résultat |
| 9 | Recherche par genre | "dystopie" | 1 résultat |
| 10 | Requête vide | "" | Tous les livres |
| 11 | Aucun résultat | "zzz_inexistant" | Liste vide |
| 12 | Recherche par ISBN | "978-2-07-040850-4" | 1 résultat exact |

#### CreateAsync (4 tests)

| # | Scénario | Condition | Résultat attendu |
|---|---|---|---|
| 13 | Créer livre valide | Titre + Auteur présents | ID > 0 assigné |
| 14 | Source forcée | Source = "tentative" | Source = "local" |
| 15 | Date auto | Nouveau livre | CreatedAt ≈ maintenant |
| 16 | Persistance | Créer puis GetById | Livre retrouvable |

#### UpdateAsync (2 tests)

| # | Scénario | Condition | Résultat attendu |
|---|---|---|---|
| 17 | Mettre à jour | Données modifiées | Nouvelles valeurs retournées |
| 18 | Livre inexistant | ID = 9999 | null |

#### DeleteAsync (3 tests)

| # | Scénario | Condition | Résultat attendu |
|---|---|---|---|
| 19 | Supprimer existant | ID valide | true + GetById → null |
| 20 | Supprimer inexistant | ID = 9999 | false |
| 21 | Isolation | Supprimer 1 livre | count - 1 |

#### ToggleAvailabilityAsync (4 tests)

| # | Scénario | Condition | Résultat attendu |
|---|---|---|---|
| 22 | Disponible → Emprunté | IsAvailable = true | IsAvailable = false |
| 23 | Emprunté → Disponible | IsAvailable = false | IsAvailable = true |
| 24 | Double toggle | 2 appels successifs | Retour à l'état initial |
| 25 | Inexistant | ID = 9999 | null |

---

### 🌐 OpenLibraryService — Tests Unitaires (7 tests)

| # | Scénario | Technique | Résultat attendu |
|---|---|---|---|
| 26 | Désérialisation correcte | Mock HTTP 200 | 2 livres mappés |
| 27 | URL couverture | cover_i = 12345 | URL complète construite |
| 28 | Pas de couverture | cover_i absent | CoverUrl = null |
| 29 | Réponse vide | numFound=0 | Liste vide |
| 30 | Erreur HTTP 500 | Mock HTTP 500 | Liste vide, pas d'exception |
| 31 | JSON invalide | "NOT_JSON" | Liste vide, pas d'exception |
| 32 | Auteur absent | author_name manquant | Author = "Inconnu" |

---

### ✅ BookModel — Tests de Validation (7 tests)

| # | Scénario | Règle testée | Résultat attendu |
|---|---|---|---|
| 33 | Livre complet valide | Tous les [Required] OK | 0 erreur |
| 34 | Titre vide | [Required] | 1 erreur sur "Title" |
| 35 | Auteur vide | [Required] | 1 erreur sur "Author" |
| 36 | Titre > 200 chars | [StringLength(200)] | 1 erreur sur "Title" |
| 37 | Année valide (2023) | [Range(1000, 2100)] | 0 erreur |
| 38-40 | Année invalide (999, 2101, 0) | [Range] | 1 erreur sur "PublishedYear" |
| 41 | Champs optionnels null | Optionnels | 0 erreur |

---

### 🔗 BooksController — Tests d'Intégration (15 tests)

| # | Scénario | Endpoint | Code HTTP attendu |
|---|---|---|---|
| 42 | Liste tous les livres | GET /api/books | 200 OK |
| 43 | Content-Type JSON | GET /api/books | application/json |
| 44 | IDs valides (> 0) | GET /api/books | 200 + IDs > 0 |
| 45 | Livre existant | GET /api/books/1 | 200 OK |
| 46 | Livre inexistant | GET /api/books/9999 | 404 Not Found |
| 47 | Recherche avec résultats | GET /search?q=dune | 200 + livre Dune |
| 48 | Recherche vide | GET /search?q= | 200 + tous les livres |
| 49 | Recherche sans résultats | GET /search?q=xxx | 200 + liste vide |
| 50 | Créer livre valide | POST /api/books | 201 Created |
| 51 | Créer sans titre | POST /api/books | 400 Bad Request |
| 52 | Créer puis récupérer | POST + GET | 201 puis 200 |
| 53 | Mettre à jour | PUT /api/books/{id} | 200 OK |
| 54 | Mettre à jour inexistant | PUT /api/books/9999 | 404 Not Found |
| 55 | Supprimer | DELETE /api/books/{id} | 204 No Content |
| 56 | Supprimer puis GET | DELETE + GET | 204 puis 404 |

---

### 🌍 OpenLibraryController — Tests d'Intégration (5 tests)

| # | Scénario | Endpoint | Code HTTP attendu |
|---|---|---|---|
| 57 | Recherche sans paramètre | GET /search | 400 Bad Request |
| 58 | Recherche paramètre vide | GET /search?q= | 400 Bad Request |
| 59 | Import livre valide | POST /import | 201 Created |
| 60 | Import sans titre | POST /import | 400 Bad Request |
| 61 | Import puis récupération | POST + GET /books | 201 puis 200 |

---

## Comment lancer les tests

### Tous les tests
```bash
cd Library.Tests
dotnet test
```

### Avec rapport détaillé
```bash
dotnet test --verbosity normal
```

### Seulement les tests unitaires
```bash
dotnet test --filter "FullyQualifiedName~Unit"
```

### Seulement les tests d'intégration
```bash
dotnet test --filter "FullyQualifiedName~Integration"
```

### Avec couverture de code
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Résultat attendu
```
Test run for Library.Tests.dll
...
Passed! - Failed: 0, Passed: 52, Skipped: 0, Total: 52
Duration: ~3s
```

---

## Outils et librairies utilisées

| Outil | Version | Rôle |
|---|---|---|
| **xUnit** | 2.6.2 | Framework de test principal |
| **FluentAssertions** | 6.12 | Assertions lisibles (`Should().Be()`) |
| **Moq** | 4.20 | Mock d'interfaces et HttpClient |
| **EF Core InMemory** | 8.0 | Base de données sans fichier |
| **WebApplicationFactory** | 8.0 | Serveur HTTP en mémoire pour l'intégration |
| **coverlet** | 6.0 | Mesure la couverture de code |

---

## Pourquoi ces deux types de tests ?

### Tests unitaires → filets de sécurité rapides
- S'exécutent en **millisecondes**
- Détectent les bugs **avant** de lancer l'application
- Parfaits pour tester la **logique métier** en isolation

### Tests d'intégration → validation du comportement réel
- Testent le **vrai pipeline ASP.NET Core**
- Détectent les bugs de **routage, sérialisation, CORS**
- Garantissent que les couches **fonctionnent ensemble**

> **Règle d'or** : les tests unitaires disent *"ça marche isolément"*,  
> les tests d'intégration disent *"ça marche ensemble"*.