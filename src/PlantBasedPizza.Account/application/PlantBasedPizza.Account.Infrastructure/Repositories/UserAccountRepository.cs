using System.Security.Cryptography;
using System.Text;
using PlantBasedPizza.Account.Core.Entities;
using PlantBasedPizza.Account.Core.Exceptions;
using PlantBasedPizza.Account.Core.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
namespace PlantBasedPizza.Account.Infrastructure.Repositories;

public class UserAccountRepository : IUserAccountRepository
{
    private readonly IMongoCollection<UserAccount> _accounts;
    private readonly ILogger _logger;
    public UserAccountRepository(MongoClient client, ILogger<UserAccountRepository> logger)
    {
        var database = client.GetDatabase("SimplePizzaWinkel-Accounts");
        _accounts = database.GetCollection<UserAccount>("accounts");
        _logger = logger;
    }
    
    public async Task<UserAccount> CreateAccount(UserAccount userAccount)
    {
        var queryBuilder = Builders<UserAccount>.Filter.Eq(p => p.EmailAddress, userAccount.EmailAddress);

        var existingAccount = await _accounts.Find(queryBuilder).FirstOrDefaultAsync().ConfigureAwait(false);

        if (existingAccount is not null)
        {
            var errorMsg = $"{userAccount.EmailAddress} already exists";
            throw new UserExistsException(errorMsg);
        }
        
        this._logger.LogInformation("Creating new account for {EmailAddress}", userAccount.EmailAddress);
        await _accounts.InsertOneAsync(userAccount).ConfigureAwait(false);

        return userAccount;
    }

    public async Task<UserAccount> ValidateCredentials(string emailAddress, string password)
    {
        var queryBuilder = Builders<UserAccount>
            .Filter;

        var filter = queryBuilder.Eq(account => account.EmailAddress, emailAddress) &
                     queryBuilder.Eq(account => account.Password, HashPassword(password));

        var account = await _accounts.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);

        if (account == null)
        {
            throw new LoginFailedException();
        }

        return account;
    }

    public async Task SeedInitialUser()
    {
        try
        {
            await CreateAccount(UserAccount.Create("admin@simplepizzawinkel.com", "AdminAccount!23", AccountType.Admin));
        }
        catch (UserExistsException)
        {
            // There is no problem if this fails.
        }
    }

    // Note: This hashing algorithm may not be suitable for production scenarios with real user data
    private string HashPassword(string password)
    {
        // Create a new instance of the SHA512 hash algorithm
        using SHA512 sha512Hash = SHA512.Create();
        // Convert the input string to a byte array and compute the hash
        byte[] data = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

        // Create a new Stringbuilder to collect the bytes
        // and create a string
        StringBuilder builder = new StringBuilder();

        // Loop through each byte of the hashed data 
        // and format each one as a hexadecimal string
        for (int i = 0; i < data.Length; i++)
        {
            builder.Append(data[i].ToString("x2"));
        }

        // Return the hashed string
        return builder.ToString();
    }

}