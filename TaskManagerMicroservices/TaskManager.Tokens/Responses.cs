namespace TaskManager.Tokens
{
    public class RefreshTokenResponse
    {
        public bool IsAnyIssue { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }

    }

    
}
