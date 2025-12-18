using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Common.DTOs
{
    public class TaskItemDto
    {
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string AssignToUsername { get; set; }
        public string Content { get; set; }
        public bool IsShared { get; set; }
        public bool IsCompleted { get; set; }
        public string GroupName { get; set; }
        public int Priority { get; set; }
        public string TaskTitle { get; set; }

        /* FOR FRONTEND ACTIONS */
        public bool IsRemovable { get; set; }
        public bool IsCompletable { get; set; }
    }

    public record CreateTaskItemDto
    (
        [Required] string TaskId,
        [Required][MaxLength(300)] string Content,
        string AssignToUsername,
        string? GroupName,
        [Required] string TaskTitle,
        string? SprintId
    );

    public record SetPriorityToTaskItemDto
    (
        [Required] string TaskId,
        [Required] string TaskItemId,
        [Range(0, 3)] int Priority,
        [Required] string GroupName
    );

    public record SetTaskItemAsCompleted
    (
        [Required] string TaskItemId,
        List<bool> TaskItemsStatus,
        bool IsSprint
    );

    public record MarkTaskItemAsCompletedDto
    (
        [Required] string TaskItemId,
        [Required] string TaskId,
        string GroupName,
        string? SprintId
    );

    public record DeleteTaskItemDto
    (
        [Required] string TaskItemId,
        string? GroupName,
        [Required] string TaskId
    );

    public record AskToAssistantDto(
        string? PreviousResponse, 
        string? Prompt,
        string TaskContent
    );

    public record UpdateTaskItemDto(
        [Required] string TaskId,
        [Required] string TaskItemId,
        [Required] string Content,
        string? AssignTo
    );
}
