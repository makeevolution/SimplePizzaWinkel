using PlantBasedPizza.Payments.Entities;

namespace PlantBasedPizza.Payments.Core.Adapters.Repositories;

public interface IDeadLetterRepository
{
    Task StoreAsync(DeadLetterMessage message);

    Task<IEnumerable<DeadLetterMessage>> GetDeadLetterItems();
}