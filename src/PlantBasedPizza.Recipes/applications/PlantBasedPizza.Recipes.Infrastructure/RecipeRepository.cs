using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PlantBasedPizza.Recipes.Core.Entities;

namespace PlantBasedPizza.Recipes.Infrastructure;

public class RecipeRepository : IRecipeRepository
{
    private readonly IMongoCollection<Recipe> _recipes;
    private readonly ILogger<RecipeRepository> _logger;

    public RecipeRepository(MongoClient client, ILogger<RecipeRepository> logger)
    {
        var database = client.GetDatabase("SimplePizzaWinkel");
        _recipes = database.GetCollection<Recipe>("recipes");
        _logger = logger;
    }
    
    public async Task<Recipe?> Retrieve(string recipeIdentifier)
    {
        _logger.LogInformation("RecipeIdentifier: {RecipeIdentifier}", recipeIdentifier);
        Activity.Current?.AddTag("RecipeIdentifier", recipeIdentifier);
        _logger.LogInformation("Retrieving recipe {RecipeIdentifier}", recipeIdentifier);
        var queryBuilder = Builders<Recipe>.Filter.Eq(p => p.RecipeIdentifier, recipeIdentifier);

        var recipe = await _recipes.Find(queryBuilder).FirstOrDefaultAsync();
        Activity.Current?.AddTag("Recipe: ", recipe);
        return recipe;
    }

    public async Task<IEnumerable<Recipe>> List()
    {
        var recipes = await _recipes.Find(p => true).ToListAsync();

        return recipes;
    }
    

    public async Task<bool> Exists(Recipe recipe)
    {
        var existingRecipe = await this.Retrieve(recipe.RecipeIdentifier);
        
        return existingRecipe is not null;
    }

    public async Task Add(Recipe recipe)
    {
        var recipeExists = await this.Exists(recipe);
        if (recipeExists)
        {
            return;
        }
        
        await _recipes.InsertOneAsync(recipe).ConfigureAwait(false);
    }

    public async Task Update(Recipe recipe)
    {
        var queryBuilder = Builders<Recipe>.Filter.Eq(ord => ord.RecipeIdentifier, recipe.RecipeIdentifier);

        await _recipes.ReplaceOneAsync(
            queryBuilder,
            recipe);
    }

    public async Task SeedRecipes()
    {

        var pesto = new Recipe(RecipeCategory.Pizza, "pesto", "Pesto", 8.99M);
        pesto.AddIngredient("Tomatoes", 1);
        pesto.AddIngredient("Cheese", 6);
        
        var marg = new Recipe(RecipeCategory.Pizza, "margherita", "Margherita", 4.99M);
        marg.AddIngredient("Tomatoes", 1);
        marg.AddIngredient("Cheese", 6);
        
        var pepperoni = new Recipe(RecipeCategory.Pizza, "pepperoni", "Pepperoni", 10.99M);
        pepperoni.AddIngredient("Tomatoes", 1);
        pepperoni.AddIngredient("Cheese", 6);
        pepperoni.AddIngredient("Pepperoni", 20);
        
        var veggieDeluxe = new Recipe(RecipeCategory.Pizza, "vegetarian", "Vegetarian", 7.99M);
        veggieDeluxe.AddIngredient("Tomatoes", 1);
        veggieDeluxe.AddIngredient("Cheese", 6);
        veggieDeluxe.AddIngredient("Mushroom", 6);
        veggieDeluxe.AddIngredient("Red Peppers", 6);
        veggieDeluxe.AddIngredient("Green Peppers", 6);
        veggieDeluxe.AddIngredient("Olives", 12);
        
        var pineapple = new Recipe(RecipeCategory.Pizza, "pineapple", "Pineapple", 100.99M);
        pineapple.AddIngredient("Tomatoes", 1);
        pineapple.AddIngredient("Cheese", 6);
        
        var spicy = new Recipe(RecipeCategory.Pizza, "spicy-flaming-hot", "Spicy Flaming Hot", 9.99M);
        spicy.AddIngredient("Tomatoes", 1);
        spicy.AddIngredient("Cheese", 6);

        var fries = new Recipe(RecipeCategory.Sides, "fries", "Fries", 3.99M);
        fries.AddIngredient("Potatoes", 4);
        
        var nuggets = new Recipe(RecipeCategory.Sides, "nuggets", "Nuggets", 5.99M);
        nuggets.AddIngredient("Potatoes", 4);
        
        await Add(pesto);
        await Add(marg);
        await Add(pepperoni);
        await Add(veggieDeluxe);
        await Add(pineapple);
        await Add(spicy);
        await Add(fries);
        await Add(nuggets);

        var softDrinks = new[] { "Beer", "Wine", "Gin", "Vodka", "Baileys", "Whiskey", "Rum", "Tequila", "Cider", "Soda" };

        foreach (var drink in softDrinks)
        {
            await Add(new Recipe(RecipeCategory.Drinks, drink.ToLower().Replace(" ", "-"), drink, 1.50M));
        }
    }
}