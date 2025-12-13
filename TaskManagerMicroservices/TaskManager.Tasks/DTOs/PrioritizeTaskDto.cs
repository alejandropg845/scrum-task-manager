using System.ComponentModel.DataAnnotations;

namespace TaskManager.Tasks.DTOs
{
    public record PrioritizeTaskDto
    (
        [Required] string GroupName,

        [Required] string TaskId,

        [Required]
        [Range(1, 3)]
        int Priority

    );
}
