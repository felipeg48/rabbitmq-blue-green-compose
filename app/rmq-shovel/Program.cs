using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using rmq_experian;

var username = "admin";
var password = "admin";

var shovel = new ShovelRequest()
{
    Value = new Value()
    {
        SrcQueue = "simple",
        DestQueue = "simple",
        DestUri = $"amqp://{username}:{password}@green-haproxy:5672",
        SrcPrefetchCount = 1,
        SrcDeleteAfter = "queue-length"
    }
};

var shovelJson = JsonSerializer.Serialize(shovel);
Console.WriteLine(shovelJson);

// Create the Shovel
var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
var shovelRequestUri = "/api/parameters/shovel/%2F/my-shovel";

var httpClient = new HttpClient()
{
    BaseAddress = new UriBuilder("http", "localhost", 15672).Uri,
    DefaultRequestHeaders =
    {
        Authorization = new AuthenticationHeaderValue("Basic", auth)
    }
};

var request = new HttpRequestMessage(HttpMethod.Put, shovelRequestUri);
request.Content = new StringContent(shovelJson, Encoding.UTF8, "application/json");

var response = await httpClient.SendAsync(request);
response.EnsureSuccessStatusCode();
Console.WriteLine(await response.Content.ReadAsStringAsync());