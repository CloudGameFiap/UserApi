using CloudGame.Domain.Commom;
using CloudGame.Domain.Entities;
using CloudGame.Domain.Events.User;
using CloudGame.Domain.Handlers;
using CloudGame.Domain.Interfaces;
using CloudGame.Domain.Interfaces.Security;
using MassTransit;

namespace CloudGame.Application.Handlers.UserHandler.Create;

public sealed class CreateUserCommandHandler(
    IUserWriteOnlyRepository userWriteOnlyRepository,
    IPasswordHasher passwordHasher,
    IUserReadOnlyRepository userReadOnlyRepository,
    IPublishEndpoint publishEndpoint,
    IUnitOfWork unitOfWork) : IHandler<CreateUserCommand, CreateUserCommandResponse>
{
    public async Task<Result<CreateUserCommandResponse>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        string passwordHash = passwordHasher.CreateHash(command.Password);

        var isEmailInUse = await userReadOnlyRepository.CheckIfIsEmailBeingUsedAsync(command.Email);

        if (isEmailInUse)
        {
            return Result<CreateUserCommandResponse>.Failure([new Error("Email", "Email is already in use")]);
        }

        User newUser = new(command.Name, command.Email, passwordHash, command.BirthDate, false);

        await userWriteOnlyRepository.AddAsync(newUser);

        await unitOfWork.SaveChangesAsync();

        var userCreatedEvent = new UserCreatedEvent(newUser.Name, newUser.Email, newUser.BirthDate, newUser.Active, newUser.UpdateAt, newUser.IsAdmin);

        await publishEndpoint.Publish(userCreatedEvent, cancellationToken);

        return Result<CreateUserCommandResponse>.Success(new CreateUserCommandResponse(newUser.Id, newUser.Name, newUser.Email));
    }
}
