namespace TaskManager.Common.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(string username, string audience, string issuer);
    }
}