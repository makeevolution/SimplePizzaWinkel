namespace PlantBasedPizza.Recipes.Infrastructure;

public static class InfrastructureConstants
{
    public static string TableName => Environment.GetEnvironmentVariable("TABLE_NAME") ?? "simple-pizza-winkel";
}