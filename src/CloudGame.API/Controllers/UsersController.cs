using CloudGame.API.Extensions;
using CloudGame.Application.Handlers.UserHandler.ChangeActive;
using CloudGame.Application.Handlers.UserHandler.Create;
using CloudGame.Application.Handlers.UserHandler.Delete;
using CloudGame.Application.Handlers.UserHandler.Find;
using CloudGame.Application.Handlers.UserHandler.GetById;
using CloudGame.Application.Handlers.UserHandler.Update;
using CloudGame.Domain.Commom;
using CloudGame.Domain.Handlers;
using CloudGame.Domain.Parameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudGame.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<CreateUserCommandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateUserCommand command,
        [FromServices] IHandler<CreateUserCommand, CreateUserCommandResponse> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [Authorize()]
    [ProducesResponseType(typeof(Result<GetUserByIdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByIdAsync(
        [FromRoute] int id,
        [FromServices] IHandler<GetUserByIdQuery, GetUserByIdResponse> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery { Id = id };
        var result = await handler.HandleAsync(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(Result<Pagination<FindUsersQueryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FindAsync(
        [FromQuery] FindUsersParameter parameters,
        [FromServices] IHandler<FindUsersQuery, Pagination<FindUsersQueryResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new FindUsersQuery(parameters), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut]
    [Authorize()]
    [ProducesResponseType(typeof(Result<UpdateUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAsync(
        [FromBody] UpdateUserCommand command,
        [FromServices] IHandler<UpdateUserCommand, UpdateUserResponse> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("ChangeActiveUser")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeActiveUserAsync(
        [FromBody] ChangeActiveUserCommand command,
        [FromServices] IHandler<ChangeActiveUserCommand> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpDelete]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUserAsync(
        [FromBody] DeleteUserCommand command,
        [FromServices] IHandler<DeleteUserCommand> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.ToActionResult();
    }
}
