using System.ComponentModel.DataAnnotations;

namespace TaskManager.Tasks.DTOs
{
    public class CreateTaskDto
    {
        [Required]
        [MaxLength(90)]
        public string Title { get; set; }
        public string? GroupName { get; set; }
    }
}
