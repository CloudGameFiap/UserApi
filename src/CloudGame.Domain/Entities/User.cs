namespace CloudGame.Domain.Entities;

public sealed class User : Entity<int>
{
    public User(string name, string email, string password, DateTime birthDate, bool isAdmin)
    {
        Name = name;
        Email = email;
        SetPassword(password);
        BirthDate = birthDate;
        Active = true;
        IsAdmin = isAdmin;
    }

    public string Name { get; private set; }

    public string Email { get; private set; }

    public string Password { get; private set; }

    public DateTime BirthDate { get; private set; }

    public bool Active { get; private set; }

    public DateTime? UpdateAt { get; private set; }

    public bool IsAdmin { get; private set; }

    public void SetActive(bool active)
    {
        Active = active;
    }

    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("A senha é obrigatoria.");

        Password = passwordHash;
    }

    public void SetUpdateAt()
    {
        UpdateAt = DateTime.UtcNow;
    }

    public void UpdateUser(string name, string email, DateTime birthDate)
    {
        Name = name;
        Email = email;
        BirthDate = birthDate;
    }

    public void DeleteData()
    {
        Name = "@deleted";
        Password = "@deleted";
        Active = false;
        BirthDate = new DateTime(1950, 1, 1);
    }

    private User() { }
}