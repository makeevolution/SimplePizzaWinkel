using PlantBasedPizza.Account.Core.Entities;

namespace PlantBasedPizza.Account.Core.Interfaces;

public interface IUserAccountRepository
{
    Task<UserAccount> CreateAccount(UserAccount userAccount);

    Task<UserAccount> ValidateCredentials(string emailAddress, string password);
    
    Task SeedInitialUser();
}