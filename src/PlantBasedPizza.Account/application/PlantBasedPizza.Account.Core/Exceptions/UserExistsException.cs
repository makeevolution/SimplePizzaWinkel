using System.Diagnostics;

namespace PlantBasedPizza.Account.Core.Exceptions;

public class UserExistsException : Exception
{
    public UserExistsException(string message) : base(message)
    {
        Activity.Current?.AddException(new UserExistsException(message)); 
    }
}