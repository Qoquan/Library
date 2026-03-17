// =============================================================
// Fichier : Library.Tests/Unit/BookServiceTests.cs
// Rôle    : Tests unitaires pour UserBookStore.
//           Remplace les tests de BookService (EF Core)
//           maintenant que la logique est dans UserBookStore.
// =============================================================

using FluentAssertions;
using Library.API.Services;
using Library.Shared.Models;

namespace Library.Tests.Unit
{
    public class BookServiceTests
    {
        private readonly IUserBookStore _store;
        private const int UserId = 1;

        public BookServiceTests()
        {
            _store = new UserBookStore();
        }

        private static Book MakeBook(string title = "Test", string author = "Auteur") =>
            new Book { Title = title, Author = author, IsAvailable = true, Genre = "Fiction" };

        // ── GetAll ─────────────────────────────────────────────

        [Fact(DisplayName = "GetAll — retourne liste vide si aucun livre")]
        public void GetAll_Empty_ReturnsEmpty()
        {
            _store.GetAll(UserId).Should().BeEmpty();
        }

        [Fact(DisplayName = "GetAll — retourne les livres de l'utilisateur")]
        public void GetAll_AfterCreate_ReturnsBooks()
        {
            _store.Create(UserId, MakeBook("Alpha"));
            _store.Create(UserId, MakeBook("Beta"));
            _store.GetAll(UserId).Should().HaveCount(2);
        }

        [Fact(DisplayName = "GetAll — triés par titre alphabétique")]
        public void GetAll_ReturnsSortedByTitle()
        {
            _store.Create(UserId, MakeBook("Zorro"));
            _store.Create(UserId, MakeBook("Alpha"));
            var titles = _store.GetAll(UserId).Select(b => b.Title).ToList();
            titles.Should().BeInAscendingOrder();
        }

        // ── GetById ────────────────────────────────────────────

        [Fact(DisplayName = "GetById — retourne null si ID inexistant")]
        public void GetById_NotFound_ReturnsNull()
        {
            _store.GetById(UserId, 9999).Should().BeNull();
        }

        [Fact(DisplayName = "GetById — retourne le bon livre")]
        public void GetById_Found_ReturnsBook()
        {
            var created = _store.Create(UserId, MakeBook("Trouvé"));
            _store.GetById(UserId, created.Id).Should().NotBeNull();
            _store.GetById(UserId, created.Id)!.Title.Should().Be("Trouvé");
        }

        // ── Search ─────────────────────────────────────────────

        [Fact(DisplayName = "Search — vide retourne tous les livres")]
        public void Search_EmptyQuery_ReturnsAll()
        {
            _store.Create(UserId, MakeBook("Livre A"));
            _store.Create(UserId, MakeBook("Livre B"));
            _store.Search(UserId, "").Should().HaveCount(2);
        }

        [Fact(DisplayName = "Search — filtre par titre (insensible casse)")]
        public void Search_ByTitle_ReturnsMatch()
        {
            _store.Create(UserId, MakeBook("Dune"));
            _store.Create(UserId, MakeBook("1984"));
            _store.Search(UserId, "dune").Should().HaveCount(1);
        }

        [Fact(DisplayName = "Search — aucun résultat si pas de correspondance")]
        public void Search_NoMatch_ReturnsEmpty()
        {
            _store.Create(UserId, MakeBook("Dune"));
            _store.Search(UserId, "zzzzz").Should().BeEmpty();
        }

        // ── Create ─────────────────────────────────────────────

        [Fact(DisplayName = "Create — attribue un ID > 0")]
        public void Create_AssignsPositiveId()
        {
            var created = _store.Create(UserId, MakeBook());
            created.Id.Should().BeGreaterThan(0);
        }

        [Fact(DisplayName = "Create — définit CreatedAt automatiquement")]
        public void Create_SetsCreatedAt()
        {
            var before = DateTime.UtcNow.AddSeconds(-1);
            var created = _store.Create(UserId, MakeBook());
            created.CreatedAt.Should().BeAfter(before);
        }

        [Fact(DisplayName = "Create — source vide devient 'local'")]
        public void Create_EmptySource_SetsLocal()
        {
            var book = MakeBook();
            book.Source = "";
            var created = _store.Create(UserId, book);
            created.Source.Should().Be("local");
        }

        [Fact(DisplayName = "Create — source 'openlibrary' est préservée")]
        public void Create_OpenlibrarySource_IsPreserved()
        {
            var book = MakeBook();
            book.Source = "openlibrary";
            var created = _store.Create(UserId, book);
            created.Source.Should().Be("openlibrary");
        }

        // ── Update ─────────────────────────────────────────────

        [Fact(DisplayName = "Update — retourne null si livre inexistant")]
        public void Update_NotFound_ReturnsNull()
        {
            _store.Update(UserId, 9999, MakeBook()).Should().BeNull();
        }

        [Fact(DisplayName = "Update — modifie les champs correctement")]
        public void Update_ModifiesFields()
        {
            var created = _store.Create(UserId, MakeBook("Avant"));
            var updated = _store.Update(UserId, created.Id, MakeBook("Après"));
            updated!.Title.Should().Be("Après");
        }

        [Fact(DisplayName = "Update — ne change pas l'ID ni CreatedAt")]
        public void Update_PreservesIdAndCreatedAt()
        {
            var created = _store.Create(UserId, MakeBook());
            var originalDate = created.CreatedAt;
            var updated = _store.Update(UserId, created.Id, MakeBook("Nouveau titre"));
            updated!.Id.Should().Be(created.Id);
            updated.CreatedAt.Should().Be(originalDate);
        }

        // ── Delete ─────────────────────────────────────────────

        [Fact(DisplayName = "Delete — retourne false si livre inexistant")]
        public void Delete_NotFound_ReturnsFalse()
        {
            _store.Delete(UserId, 9999).Should().BeFalse();
        }

        [Fact(DisplayName = "Delete — retourne true et supprime le livre")]
        public void Delete_Found_ReturnsTrueAndRemoves()
        {
            var created = _store.Create(UserId, MakeBook());
            _store.Delete(UserId, created.Id).Should().BeTrue();
            _store.GetById(UserId, created.Id).Should().BeNull();
        }

        // ── Toggle ─────────────────────────────────────────────

        [Fact(DisplayName = "Toggle — inverse la disponibilité")]
        public void Toggle_InvertsAvailability()
        {
            var created = _store.Create(UserId, MakeBook());
            var original = created.IsAvailable;
            var toggled = _store.ToggleAvailability(UserId, created.Id);
            toggled!.IsAvailable.Should().Be(!original);
        }

        [Fact(DisplayName = "Toggle — retourne null si livre inexistant")]
        public void Toggle_NotFound_ReturnsNull()
        {
            _store.ToggleAvailability(UserId, 9999).Should().BeNull();
        }

        // ── Isolation par utilisateur ──────────────────────────

        [Fact(DisplayName = "Isolation — deux users ont des bibliothèques séparées")]
        public void TwoUsers_HaveSeparateLibraries()
        {
            _store.Create(1, MakeBook("Livre User 1"));
            _store.Create(2, MakeBook("Livre User 2"));

            _store.GetAll(1).Should().OnlyContain(b => b.Title == "Livre User 1");
            _store.GetAll(2).Should().OnlyContain(b => b.Title == "Livre User 2");
        }

        [Fact(DisplayName = "Isolation — delete user 1 n'affecte pas user 2")]
        public void Delete_User1_DoesNotAffectUser2()
        {
            var b1 = _store.Create(1, MakeBook("Livre Commun"));
            _store.Create(2, MakeBook("Livre Commun"));

            _store.Delete(1, b1.Id);

            _store.GetAll(1).Should().BeEmpty();
            _store.GetAll(2).Should().HaveCount(1);
        }
    }
}