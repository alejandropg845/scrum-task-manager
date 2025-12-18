using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TaskManager.Common.DTOs
{
    public record BeginSprintDto
    (
        [Required] string GroupName,
        [Required] List<string> TasksIds,
        [Required] int WeeksNumber,
        [MaxLength(100)] string? SprintName
    );

    public class ToSprintDto
    {
        [JsonPropertyName("expirationTime")]
        public DateTimeOffset? ExpirationTime { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("sprintNumber")]
        public int SprintNumber { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("sprintName")]
        public string? SprintName { get; set; }

        [JsonPropertyName("tasks")]
        public List<UserTaskDto> Tasks { get; set; }
    }


    public record SprintInfoToCreate
    (
        string Groupname,
        int SprintsNumber
    );

    public record SprintToComplete(
        string CompletedSprintId, 
        string NewSprintId,
        string Groupname, 
        int SprintNumber
    );
}
