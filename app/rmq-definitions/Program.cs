using System.Net.Http.Headers;
using System.Text;
using rmq_definitions;

// Struct Definitions
var blueCluster = new RabbitMQCluster()
{
    host = "localhost",
    port = 15672,
    username = "admin",
    password = "admin"
};

var greenCluster = new RabbitMQCluster(){
    host = "localhost",
    port = 15682,
    username = "admin",
    password = "admin"
};



// URI Definitions
string definitionsRequestUri = "/api/definitions";

// Cluster 1
HttpClient httpBlueClient = new HttpClient()
{
    BaseAddress = new UriBuilder("http", blueCluster.host, blueCluster.port).Uri,
    DefaultRequestHeaders =
    {
        Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{blueCluster.username}:{blueCluster.password}")))
    }
};

HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, definitionsRequestUri);
request.Headers.Add("Accept", "application/json");

HttpResponseMessage response = await httpBlueClient.SendAsync(request);
response.EnsureSuccessStatusCode();

string definitions = response.Content.ReadAsStringAsync().Result;
File.WriteAllText("definitions.json", definitions);

Console.WriteLine("Definitions saved to definitions.json");

// Cluster 2
HttpClient httpGreenClient = new HttpClient()
{
    BaseAddress = new UriBuilder("http", greenCluster.host, greenCluster.port).Uri,
    DefaultRequestHeaders =
    {
        Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{greenCluster.username}:{greenCluster.password}")))
    }
};

var clusterDefinitions = File.ReadAllText("definitions.json");

request = new HttpRequestMessage(HttpMethod.Post, definitionsRequestUri);
request.Content = new StringContent(clusterDefinitions, Encoding.UTF8, "application/json");

response = await httpGreenClient.SendAsync(request);
response.EnsureSuccessStatusCode();

Console.WriteLine("Definitions restored on green cluster");