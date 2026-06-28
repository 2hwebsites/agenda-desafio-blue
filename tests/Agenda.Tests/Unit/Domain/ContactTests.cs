using Agenda.Domain.Entities;
using Shouldly;

namespace Agenda.Tests.Unit.Domain;

public class ContactTests
{
    [Fact]
    public void Create_ValidData_SetsPropertiesAndNormalizesValues()
    {
        var contact = Contact.Create("  João Silva  ", "  JOAO@EXAMPLE.COM  ", "  (11) 91234-5678  ");

        contact.Name.ShouldBe("João Silva");
        contact.Email.ShouldBe("joao@example.com");
        contact.Phone.ShouldBe("(11) 91234-5678");
        contact.Id.ShouldNotBe(Guid.Empty);
        contact.IsDeleted.ShouldBeFalse();
        contact.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public void Create_NullPhone_StoresNull()
    {
        var contact = Contact.Create("Test", "test@test.com");
        contact.Phone.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_ThrowsArgumentException(string name)
    {
        Should.Throw<ArgumentException>(() => Contact.Create(name, "test@test.com"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyEmail_ThrowsArgumentException(string email)
    {
        Should.Throw<ArgumentException>(() => Contact.Create("Test", email));
    }

    [Fact]
    public void Update_ValidData_ChangesNameEmailAndPhone()
    {
        var contact = Contact.Create("Original", "original@test.com", null);

        contact.Update("  Updated Name  ", "  UPDATED@TEST.COM  ", "(21) 98765-4321");

        contact.Name.ShouldBe("Updated Name");
        contact.Email.ShouldBe("updated@test.com");
        contact.Phone.ShouldBe("(21) 98765-4321");
    }

    [Fact]
    public void Update_NullPhone_ClearsPhone()
    {
        var contact = Contact.Create("Test", "test@test.com", "(11) 91234-5678");
        contact.Update("Test", "test@test.com", null);
        contact.Phone.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_EmptyName_ThrowsArgumentException(string name)
    {
        var contact = Contact.Create("Test", "test@test.com");
        Should.Throw<ArgumentException>(() => contact.Update(name, "test@test.com", null));
    }

    [Fact]
    public void MarkAsDeleted_SetsIsDeletedTrue()
    {
        var contact = Contact.Create("Test", "test@test.com");

        contact.MarkAsDeleted();

        contact.IsDeleted.ShouldBeTrue();
    }

    [Fact]
    public void Create_TwoContacts_HaveDistinctIds()
    {
        var a = Contact.Create("A", "a@test.com");
        var b = Contact.Create("B", "b@test.com");
        a.Id.ShouldNotBe(b.Id);
    }
}
