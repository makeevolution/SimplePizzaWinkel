using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using PlantBasedPizza.Account.Core.Entities;
using PlantBasedPizza.Account.Core.Exceptions;
using PlantBasedPizza.Account.Infrastructure.Services;

namespace PlantBasedPizza.Account.Api.Endpoints;

[HttpPost("/register")]
[AllowAnonymous]
public class RegisterUserEndpoint(UserAccountService userAccountService, ILogger<RegisterStaffEndpoint> logger)
    : Endpoint<RegisterUserCommand, RegisterResponse?>
{
    public override async Task<RegisterResponse?> HandleAsync(
        RegisterUserCommand request,
        CancellationToken ct)
    {
        try
        {
            var registerResponse = await userAccountService.Register(request, AccountType.User);

            Response = registerResponse;
            return registerResponse;
        }
        catch (UserExistsException ex)
        {
            logger.LogError(ex, "Failed to register user");
            await SendErrorsAsync(400, ct);
            return null;
        }
    }
}