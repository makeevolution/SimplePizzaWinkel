namespace PlantBasedPizza.Account.Core.Entities;

public class RegisterUserCommand
{
    public string EmailAddress { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
}