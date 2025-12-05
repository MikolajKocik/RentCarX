namespace RentCarX.Domain.Interfaces.UserContext;

public interface IUserContextService
{
    Guid UserId { get; }
    string Email { get; }
}
