using System.Text.Json.Serialization;

namespace rmq_experian;

public record struct ShovelRequest()
{
    [JsonPropertyName("value")]
    public Value Value { get; init; }
}

/*
   {
     "value": {
       "src-protocol": "amqp091",
       "src-uri":  "amqp://localhost",
       "src-queue":  "source-queue",
       "src-delete-after": "never",
       "dest-protocol": "amqp091",
       "dest-uri": "amqp://remote.rabbitmq.local",
       "dest-queue": "destination-queue",
       "ack-mode": "on-confirm",
       "src-prefetch-count": 1000,
       "dest-add-forward-headers": true    
     }
   }
 */
public record struct Value()
{
    [JsonPropertyName("src-protocol")] public string SrcProtocol { get; init; } = "amqp091";
    [JsonPropertyName("src-uri")] public string SrcUri { get; init; } = "amqp://";
    [JsonPropertyName("src-queue")] public string SrcQueue { get; init; } = "source-queue";
    [JsonPropertyName("src-delete-after")] public string SrcDeleteAfter { get; init; } = "never";
    
    [JsonPropertyName("dest-protocol")] public string DestProtocol { get; init; } = "amqp091";
    [JsonPropertyName("dest-uri")] public string DestUri { get; init; } = "amqp://";
    [JsonPropertyName("dest-queue")] public string DestQueue { get; init; } = "destination-queue";
    [JsonPropertyName("ack-mode")] public string AckMode { get; init; } = "on-confirm";
    [JsonPropertyName("src-prefetch-count")] public int SrcPrefetchCount { get; init; } = 1000;
    [JsonPropertyName("dest-add-forward-headers")] public bool DestAddForwardHeaders { get; init; } = true;
}