namespace TaskManager.Tasks.Interfaces
{
    public interface ITaskClients
    {
        IGroupsClient Groups { get; }
        IGroupsRolesClient GroupRoles { get; }
        ITaskItemsClient TaskItems { get; }
    }
}
