namespace rmq_definitions;
public record struct RabbitMQCluster(){
    public string host { get; init; }
    public int port { get; init; }
    public string username { get; init; }
    public string password { get; init; }

}