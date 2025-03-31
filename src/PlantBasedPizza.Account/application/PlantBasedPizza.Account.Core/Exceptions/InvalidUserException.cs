namespace PlantBasedPizza.Account.Core.Exceptions;

public class InvalidUserException : Exception
{
    public InvalidUserException(string reason)
    {
        Reason = reason;
    }
    
    public string Reason { get; set; }
}