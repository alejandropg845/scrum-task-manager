using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Common.DTOs
{
    public record CreateRetrospectiveDto
    (
        [Required] string GroupName,
        [Required] string SprintId,
        [Required] [Range(0, 5)] int Rating,
        [Required] [MaxLength(200)] string Feedback,
        [Required] string Name
    );

    public record ToRetrospectiveDto
    (
        int Rating,
        string Feedback,
        string Name,
        DateTimeOffset SubmitedAt
    );
}
