using Newtonsoft.Json;

namespace Server;

public class User
{
    [JsonProperty("Username")]
    public string Userame { get; set; }
    [JsonProperty("Password")]
    public string Password { get; set; }
    [JsonProperty("Role")]
    public string Role { get; set; }
}