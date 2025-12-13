using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Common.Documents;

namespace TaskManager.Common.DTOs
{
    public class UserTaskDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string GroupName { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string Title { get; set; }
        public IReadOnlyCollection<TaskItemDto> TaskItems { get; set; }
        public string? Status { get; set; }
        public string? SprintId { get; set; }
        public string? SprintStatus { get; set; }
        public int Priority { get; set; }

        /* FRONTEND ACTIONS */
        public bool IsRemovable { get; set; }
    }

    public record SprintInfoForTask 
    (
        List<string> TasksIds,
        string SprintId
    );
}
