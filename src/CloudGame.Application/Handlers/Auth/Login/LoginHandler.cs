using CloudGame.Application.Settings;
using CloudGame.Domain.Commom;
using CloudGame.Domain.Entities;
using CloudGame.Domain.Handlers;
using CloudGame.Domain.Interfaces;
using CloudGame.Domain.Interfaces.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CloudGame.Application.Handlers.Auth.Login;

public class LoginHandler(
    IOptions<JwtSettings> jwtSettingsOption,
    IUserReadOnlyRepository userReadOnlyRepository,
    IPasswordHasher passwordHasher) : IHandler<LoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        User? user = await userReadOnlyRepository.GetByEmailAsync(command.User);

        if (user is null || !user.Active || !passwordHasher.VerifyPassword(user.Password, command.Password))
            return Result<LoginResponse>.Failure([new("LoginInvalido", "Login ou senha invalida")]);

        Claim[] claims = [
             new(ClaimTypes.Name, user.Name),
             new("UserId", user.Id.ToString()),
             new(ClaimTypes.Role, user.IsAdmin ? "admin" : "user")
        ];

        JwtSettings jwtSettings = jwtSettingsOption.Value;
        var key = Encoding.ASCII.GetBytes(jwtSettings.EncriptKey);
        var expirationDate = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiresInMinutes);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expirationDate,
            SigningCredentials =
              new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature),
            Audience = jwtSettings.Audience,
            Issuer = jwtSettings.Issuer,
        };

        JwtSecurityTokenHandler tokenHandler = new();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return Result<LoginResponse>.Success(new()
        {
            Token = tokenHandler.WriteToken(securityToken),
            ExpirationDate = expirationDate,
        });
    }
}
