namespace PlantBasedPizza.Account.Core.Entities;

public class LoginCommand
{
    public string EmailAddress { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
}