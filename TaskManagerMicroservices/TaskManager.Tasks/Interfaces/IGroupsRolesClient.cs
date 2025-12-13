namespace TaskManager.Tasks.Interfaces
{
    public interface IGroupsRolesClient
    {
        Task<string?> GetRoleNameAsync(string groupName, string username, string token);
    }
}
