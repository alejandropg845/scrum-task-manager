using System.Text.Json.Serialization;

namespace TaskManager.Sprints.Payloads
{
    public class DeleteFeedbacks
    {
        [JsonPropertyName("groupName")] public string GroupName { get; set; }
        [JsonPropertyName("sprintId")] public string SprintId { get; set; }
    }
}
