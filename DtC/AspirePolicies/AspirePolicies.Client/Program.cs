using AspirePolicies.Web;

const int clientCount = 5;
var httpClient = new HttpClient
{
    BaseAddress = new Uri("http://localhost:5466")
};
var clients =
    Enumerable.Range(1, clientCount).Select(id => new PolicyClient(id, httpClient)).ToList();

var tasks = clients.Select(c => c.GetPoliciesAsync()).ToList();
var policies = Task.WhenAll(tasks);
try
{
    await policies;
    Console.WriteLine("Received all the policies");
}
catch (Exception e)
{
    Console.WriteLine($"Failure getting policies: {e}");
}












//using System.Text.Json;

//var p = new Policy() { Name = "Name", Action = "fooBar" };

//var json = """  
//           {"Name1": "foo"}
//           """;
//try
//{
//    var p1 = Newtonsoft.Json.JsonConvert.DeserializeObject<Policy>(json);
//    Console.WriteLine(p1);
//}
//catch (Exception e)
//{
//    Console.WriteLine("Deserialization exception: " + e.Message);
//}

//public record Policy
//{
//    [Newtonsoft.Json.JsonRequired]
//    public required string Name { get; init; }
//    public string? Action { get; init; }
//}


//// // // Newtonsoft.Json case
//// // [Newtonsoft.Json.JsonRequired]
//// // try
//// // {
//// //     var p1 = Newtonsoft.Json.JsonConvert.DeserializeObject<Policy>(json);
//// //     Console.WriteLine(p1);
//// // }
//// // catch (Exception e)
//// // {
//// //     Console.WriteLine("Deserialization exception: " + e.Message);
//// // }























//// //using AspirePolicies.Web;

//// //var apiServiceUri = "http://localhost:5466";
//// //var httpClient = new HttpClient
//// //{
//// //    BaseAddress = new Uri(apiServiceUri)
//// //};

//// //const int clientCount = 5;
//// //var clients = Enumerable.Range(1, clientCount).Select(id => new PolicyClient(id, httpClient)).ToList();
//// //try
//// //{
//// //    var policies = await Task.WhenAll(clients.Select(c => c.GetPoliciesAsync()).ToList());
//// //    Console.WriteLine($"Successfully retrieved all {policies.Length} policies for {clientCount} clients!");
//// //}
//// //catch (Exception e)
//// //{
//// //    Console.WriteLine($"Failure retrieving all the policies for {clientCount} clients! " + e);
//// //}

//// string? str = null;

//// var exception = CreateException(null, null);

//// Exception CreateException(string message, string? details)
//// {
////     ArgumentNullException.ThrowIfNull(message);

////     return new Exception($"Message: {message}, Details: {details}");
//// }
