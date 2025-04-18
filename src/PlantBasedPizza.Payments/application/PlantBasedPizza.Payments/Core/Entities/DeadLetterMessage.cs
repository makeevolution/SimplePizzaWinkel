namespace PlantBasedPizza.Payments.Core.Entities;

public record DeadLetterMessage
{
    public string EventId { get; set; }
    
    public string EventType { get; set; }
    
    public string EventData { get; set; }
    
    public string TraceParent { get; set; }
}