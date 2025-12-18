using System.Text.Json.Serialization;

namespace TaskManager.Groups.Payloads
{
    public class DeleteGroup
    {
        [JsonPropertyName("groupName")] public string GroupName { get; set; }
        [JsonPropertyName("username")] public string Username { get; set; }
    }
}
