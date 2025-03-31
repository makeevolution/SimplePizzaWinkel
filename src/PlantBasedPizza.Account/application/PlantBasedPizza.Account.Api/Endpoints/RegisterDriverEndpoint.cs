using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using PlantBasedPizza.Account.Core.Entities;
using PlantBasedPizza.Account.Core.Exceptions;
using PlantBasedPizza.Account.Infrastructure.Services;

namespace PlantBasedPizza.Account.Api.Endpoints;

[HttpPost("/driver/register")]
[AllowAnonymous]
[Authorize(Roles = "admin,staff")]
public class RegisterDriverEndpoint(UserAccountService userAccountService, ILogger<RegisterDriverEndpoint> logger)
    : Endpoint<RegisterUserCommand, RegisterResponse?>
{
    public override async Task<RegisterResponse?> HandleAsync(
        RegisterUserCommand request,
        CancellationToken ct)
    {
        try
        {
            var registerResponse = await userAccountService.Register(request, AccountType.Driver);

            Response = registerResponse;
            return registerResponse;
        }
        catch (UserExistsException ex)
        {
            logger.LogError(ex, "Failed to register driver");
            await SendErrorsAsync(400, ct);
            return null;
        }
    }
}