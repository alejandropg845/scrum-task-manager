using System.ComponentModel.DataAnnotations;

namespace TaskManager.Common.DTOs
{
    public class ToUserDto
    {
        public string Username { get; set; }
        public string GroupName { get;set; }
        public string? GroupRole {  get; set; }
    };
    public record RegisterUserDto
    (
        [Required] [MaxLength(15)] string Username,
        [MaxLength(50)] string? Email,
        [Required] [MaxLength(20)] string Password
    );

    public record LoginUserDto
    (
        [Required] [MaxLength(15)] string Username,
        [Required] [MaxLength(20)] string Password
    );

    public record ReceiveRecoveryCodeDto
    (
        [Required] string RecoveryCode, 
        [Required] string Password1, 
        [Required] string Password2,
        [Required] string Email
    );
    public class InitialInfoDto
    {
        public bool IsGroupOwner { get; set; }
        public string? GroupName { get;set;}
        public string AvatarBgColor { get; set; }
        public TimeSpan? RemainingTime { get; set; }
        public string? GroupRole { get;set;}
        public bool IsScrum {  get;set;}
        public bool IsAllowed {  get;set;}
        public string? Status {  get;set;}
        public DateTimeOffset? ExpirationTime {  get;set;}
        public int SprintNumber { get; set; }
        public string? SprintName { get; set; }
        public bool IsError { get; set; }
        public string? FinishedSprintName { get; set; }
        public string? FinishedSprintId { get; set; }
    }


}