using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Common.DTOs
{
    public record SendMessageDto
    (
        [Required] string GroupName,
        [Required] string Message,
        [Required] string AvatarBgColor
    );
    public record GetMessagesDto
    (
        [Required] string GroupName,
        [Required] int DatePage,
        [Required] int MessagesPage,
        string? DateId,
        int SentMessages
    );
}
