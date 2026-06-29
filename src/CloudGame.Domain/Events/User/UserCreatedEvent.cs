namespace CloudGame.Domain.Events.User;

public record UserCreatedEvent(int Id, string Name, string Email, DateTime BirthDate, bool Active, DateTime? UpdateAt, bool IsAdmin);