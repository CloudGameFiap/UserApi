using CloudGame.API.Extensions;
using CloudGame.Application.Handlers.Auth.Login;
using CloudGame.Domain.Commom;
using CloudGame.Domain.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudGame.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(ILogger<AuthController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost]
    [Route("Login")]
    [ProducesResponseType(typeof(Result<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        [FromServices] IHandler<LoginCommand, LoginResponse> handler,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Login attempt for user {User}", command.User);
            var loginResult = await handler.HandleAsync(command, cancellationToken);

            logger.LogInformation("The user {User} has successfully logged in", command.User);
            return loginResult.ToActionResult();
        }
        catch (Exception ex)
        {

            logger.LogError(ex,"error login");
            return BadRequest(ex.Message);
        }       
    }
}
