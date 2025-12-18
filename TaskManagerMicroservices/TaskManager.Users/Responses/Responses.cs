using TaskManager.Common.Documents;

namespace TaskManager.Users.Responses
{
    public class LoginUserResponse
    {
        public bool UserDoesntExist { get; set; }
        public bool IsCorrect {get;set;}
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }


    public class RegisterUserResponse
    {
        public bool UserExists { get; set; }
        public bool EmailExists { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class ReceiveRecoveryCodeResponse
    {
        public bool RecoveryCodeIsOk { get; set; }
        public bool PasswordsMatch { get; set; }
        public bool IsExpired { get; set; }

    }

    public class ContinueWithGoogleResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public bool IsGoogleAuthError { get; set; }
    };
}