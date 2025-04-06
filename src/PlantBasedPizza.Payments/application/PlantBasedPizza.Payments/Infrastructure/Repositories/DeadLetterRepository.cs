using MongoDB.Driver;
using PlantBasedPizza.Payments.Core.Adapters.Repositories;
using PlantBasedPizza.Payments.Core.Entities;

namespace PlantBasedPizza.Payments.Infrastructure.Repositories;


public class DeadLetterRepository : IDeadLetterRepository
{
    private readonly IMongoCollection<DeadLetterMessage> _deadLetters;

    public DeadLetterRepository(MongoClient client)
    {
        var database = client.GetDatabase("SimplePizzaWinkel");
        _deadLetters = database.GetCollection<DeadLetterMessage>("payments_deadletters");
    }

    public async Task StoreAsync(DeadLetterMessage message)
    {
        await _deadLetters.InsertOneAsync(message);
    }

    public async Task<IEnumerable<DeadLetterMessage>> GetDeadLetterItems()
    {
        var items = await _deadLetters.FindAsync(x => true);
        
        return await items.ToListAsync();
    }
}