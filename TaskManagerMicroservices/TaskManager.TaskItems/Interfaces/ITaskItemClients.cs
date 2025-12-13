namespace TaskManager.TaskItems.Interfaces
{
    public interface ITaskItemClients
    {
        IGeminiClient Gemini { get; }
        IGroupsClient Groups { get; }
        IGroupsRolesClient GroupRoles { get; }
        ISprintsClient Sprints { get; }
        ITasksClient Tasks { get; }
    }
}
