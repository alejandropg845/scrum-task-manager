namespace TaskManager.Users.Interfaces
{
    public interface IMailRecoveryPasswordService
    {
        Task SendCodeToEmailAsync(string email, string message);
    }
}
