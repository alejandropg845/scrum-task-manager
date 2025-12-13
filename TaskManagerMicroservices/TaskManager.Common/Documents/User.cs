namespace TaskManager.Common.Documents
{
    public class User 
    {
        public string Id { get; set; }
        public string Username {get;set;}
        public string? Email { get; set;}
        public string Password {get;set;}
        public string? GroupName {get;set;}
        public string? GroupRole { get; set; }
        public string? RecoveryCode { get; set; }
        public string AvatarBgColor { get; set; }
        public DateTimeOffset? RecoveryExpirationTime { get; set; }
    }
}