namespace TaskManager.Sprints.Interfaces
{
    public interface IGroupsRolesClient
    {
        Task<bool> IsAuthorizedByGroupRoleAsync(string groupName, string token);
    }
}
