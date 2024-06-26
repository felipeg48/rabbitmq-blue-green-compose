using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace RabbitMQ.Federation
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Clusters
            var blueCluster = new RabitMQCluster
            {
                Name = "Blue",
                UserName = "admin",
                Password = "admin",
                HostName = "blue-haproxy",
                Port = 15672
            };

            var greenCluster = new RabitMQCluster
            {
                Name = "Green",
                UserName = "admin",
                Password = "admin",
                Port = 15682
            };

            // URI
            string federationAPI = "/api/parameters";

            // HTTP Clients - Green
            HttpClient httpGreenClient = new HttpClient()
            {
                BaseAddress = new UriBuilder
                {
                    Scheme = "http",
                    Host = greenCluster.HostName,
                    Port = greenCluster.Port
                }.Uri,
                DefaultRequestHeaders = 
                { 
                    Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{greenCluster.UserName}:{greenCluster.Password}"))) 
                }
            };

            // Federation Upstream - Blue <-- Green
            var federationUpstream = new RmqFderation
            {
                Name = "simple2",
                Value = new Value
                {
                    Uri = $"amqp://{blueCluster.UserName}:{blueCluster.Password}@{blueCluster.HostName}"
                }
            };

            // Serialize
            var serializeOptions = new JsonSerializerOptions
            {
                Converters = { new CustomFedeationSerializer() }
            };

            // Serialize
            var json = JsonSerializer.Serialize(federationUpstream, serializeOptions);
            Console.WriteLine(json);

            // Put
            var response = 
            new HttpRequestMessage(HttpMethod.Put, $"{federationAPI}/{federationUpstream.Component}/{HttpUtility.UrlEncode(federationUpstream.Vhost)}/{federationUpstream.Name}")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            try
            {
                var result = await httpGreenClient.SendAsync(response);
                if (result.IsSuccessStatusCode)
                {
                    Console.WriteLine("Federation Upstream Created");
                }
                else
                {
                    Console.WriteLine("Federation Upstream Failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    #region Custom Json Converter
    public class CustomFedeationSerializer : JsonConverter<RmqFderation>
    {
        public override RmqFderation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, RmqFderation value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("vhost", value.Vhost);
            writer.WriteString("component", value.Component);
            writer.WriteString("name", value.Name);

            writer.WriteStartObject("value");
            writer.WriteString("ack-mode", value.Value.AckMode);

            if (!string.IsNullOrEmpty(value.Value.ConsumerTag))
                writer.WriteString("consumer-tag", value.Value.ConsumerTag);
            
            if (!string.IsNullOrEmpty(value.Value.Exchange))
                writer.WriteString("exchange", value.Value.Exchange);

            if (value.Value.Expires > 0)
                writer.WriteNumber("expires", value.Value.Expires);
            
            if (!string.IsNullOrEmpty(value.Value.HaPolicy) && value.Value.HaPolicy != "none")
                writer.WriteString("ha-policy", value.Value.HaPolicy);

            writer.WriteNumber("max-hops", value.Value.MaxHops);

            if (value.Value.MessageTtl > 0)
                writer.WriteNumber("message-ttl", value.Value.MessageTtl);
            
            writer.WriteNumber("prefetch-count", value.Value.PrefetchCount);

            if (!string.IsNullOrEmpty(value.Value.Queue))
                writer.WriteString("queue", value.Value.Queue);
            
            writer.WriteNumber("reconnect-delay", value.Value.ReconnectDelay);
            writer.WriteBoolean("trust-user-id", value.Value.TrustUserId);
            writer.WriteString("uri", value.Value.Uri);
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }

    #endregion

    #region Models
    public record struct RabitMQCluster()
    {
        public string Name { get; init; }
        public string HostName { get; init; } = "localhost";
        public string UserName { get; init; } = "guest";
        public string Password { get; init; } = "guest";
        public int Port { get; init; } = 15672;
        public string VirtualHost { get; init; } = "/";
    }

    // Federation Upstream
    // {
    //     "vhost": "/",
    //     "component": "federation-upstream",
    //     "name": "simple",
    //     "value":
    //         {
    //             "ack-mode": "on-confirm",
    //             "consumer-tag": "my-tag",
    //             "exchange": "my-exchange",
    //             "expires": 10000,
    //             "ha-policy": "none",
    //             "max-hops": 1,
    //             "message-ttl": 10000,
    //             "prefetch-count": 1000,
    //             "queue": "my-queue",
    //             "reconnect-delay": 10,
    //             "trust-user-id": false,
    //             "uri": "amqp://admin:admin@blue-haproxy"
    //         }
    // }
    public record struct RmqFderation()
    {
        [JsonPropertyName("vhost")] public string Vhost { get; init; } = "/";
        [JsonPropertyName("component")] public string Component { get; init; } = "federation-upstream";
        [JsonPropertyName("name")] public string Name { get; init; }
        [JsonPropertyName("value")] public Value Value { get; init; }
    }

    public record struct Value()
    {
        [JsonPropertyName("ack-mode")] public string AckMode { get; init; } = "on-confirm";
        [JsonPropertyName("consumer-tag")] public string ConsumerTag { get; init; }
        [JsonPropertyName("exchange")] public string Exchange { get; init; }
        [JsonPropertyName("expires")] public int Expires { get; init; }
        [JsonPropertyName("ha-policy")] public string HaPolicy { get; init; } = "none";
        [JsonPropertyName("max-hops")] public int MaxHops { get; init; } = 1;
        [JsonPropertyName("message-ttl")] public int MessageTtl { get; init; }
        [JsonPropertyName("prefetch-count")] public int PrefetchCount { get; init; } = 1000;
        [JsonPropertyName("queue")] public string Queue { get; init; }
        [JsonPropertyName("reconnect-delay")] public int ReconnectDelay { get; init; } = 5;
        [JsonPropertyName("trust-user-id")] public bool TrustUserId { get; init; } = false;
        [JsonPropertyName("uri")] public string Uri { get; init; } 

    }
    #endregion
}
