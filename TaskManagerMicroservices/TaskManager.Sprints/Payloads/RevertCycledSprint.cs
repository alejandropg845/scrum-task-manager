using System.Text.Json.Serialization;

namespace TaskManager.Sprints.Payloads
{
    public class RevertCycledSprint
    {
        [JsonPropertyName("groupName")] public string GroupName { get; set; }
        [JsonPropertyName("completedSprintId")] public string CompletedSprintId { get; set; }
        [JsonPropertyName("newSprintId")] public string NewSprintId { get; set; }
    
    }
}
