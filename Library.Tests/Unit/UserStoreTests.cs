// =============================================================
// Fichier : Library.Tests/Unit/UserStoreTests.cs
// Rôle    : Tests unitaires pour UserStore (register/login).
// =============================================================

using FluentAssertions;
using Library.API.Services;
using Library.Shared.Models;

namespace Library.Tests.Unit
{
    public class UserStoreTests
    {
        private IUserStore MakeStore() => new UserStore();

        // ── Register ───────────────────────────────────────────

        [Fact(DisplayName = "Register — succès avec données valides")]
        public void Register_ValidData_ReturnsSuccess()
        {
            var store = MakeStore();
            var result = store.Register(new LoginRequest { Username = "alice", Password = "1234" });
            result.Success.Should().BeTrue();
            result.User.Should().NotBeNull();
            result.User!.Username.Should().Be("alice");
        }

        [Fact(DisplayName = "Register — ID est attribué automatiquement")]
        public void Register_AssignsId()
        {
            var store = MakeStore();
            var result = store.Register(new LoginRequest { Username = "bob", Password = "abcd" });
            result.User!.Id.Should().BeGreaterThan(0);
        }

        [Fact(DisplayName = "Register — username déjà pris → échec")]
        public void Register_DuplicateUsername_Fails()
        {
            var store = MakeStore();
            store.Register(new LoginRequest { Username = "alice", Password = "1234" });
            var result = store.Register(new LoginRequest { Username = "alice", Password = "5678" });
            result.Success.Should().BeFalse();
        }

        [Fact(DisplayName = "Register — username insensible à la casse")]
        public void Register_CaseInsensitive()
        {
            var store = MakeStore();
            store.Register(new LoginRequest { Username = "Alice", Password = "1234" });
            var result = store.Register(new LoginRequest { Username = "ALICE", Password = "5678" });
            result.Success.Should().BeFalse();
        }

        [Fact(DisplayName = "Register — username trop court → échec")]
        public void Register_ShortUsername_Fails()
        {
            var store = MakeStore();
            var result = store.Register(new LoginRequest { Username = "ab", Password = "1234" });
            result.Success.Should().BeFalse();
        }

        [Fact(DisplayName = "Register — mot de passe trop court → échec")]
        public void Register_ShortPassword_Fails()
        {
            var store = MakeStore();
            var result = store.Register(new LoginRequest { Username = "alice", Password = "123" });
            result.Success.Should().BeFalse();
        }

        // ── Login ──────────────────────────────────────────────

        [Fact(DisplayName = "Login — succès avec bon identifiants")]
        public void Login_ValidCredentials_ReturnsSuccess()
        {
            var store = MakeStore();
            store.Register(new LoginRequest { Username = "alice", Password = "1234" });
            var result = store.Login(new LoginRequest { Username = "alice", Password = "1234" });
            result.Success.Should().BeTrue();
            result.User!.Username.Should().Be("alice");
        }

        [Fact(DisplayName = "Login — mauvais mot de passe → échec")]
        public void Login_WrongPassword_Fails()
        {
            var store = MakeStore();
            store.Register(new LoginRequest { Username = "alice", Password = "1234" });
            var result = store.Login(new LoginRequest { Username = "alice", Password = "wrong" });
            result.Success.Should().BeFalse();
        }

        [Fact(DisplayName = "Login — utilisateur inexistant → échec")]
        public void Login_UnknownUser_Fails()
        {
            var store = MakeStore();
            var result = store.Login(new LoginRequest { Username = "nobody", Password = "1234" });
            result.Success.Should().BeFalse();
        }

        [Fact(DisplayName = "Login — insensible à la casse du username")]
        public void Login_CaseInsensitive()
        {
            var store = MakeStore();
            store.Register(new LoginRequest { Username = "Alice", Password = "1234" });
            var result = store.Login(new LoginRequest { Username = "ALICE", Password = "1234" });
            result.Success.Should().BeTrue();
        }
    }
}