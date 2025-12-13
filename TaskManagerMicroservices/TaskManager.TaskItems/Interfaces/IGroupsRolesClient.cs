namespace TaskManager.TaskItems.Interfaces
{
    public interface IGroupsRolesClient
    {
        Task<string?> GetUserGroupRoleAsync(string groupName, string token);
    }
}
