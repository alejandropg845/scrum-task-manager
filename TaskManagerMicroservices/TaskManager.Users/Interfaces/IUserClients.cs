using TaskManager.Users.Interfaces.Clients;

namespace TaskManager.Users.Interfaces
{
    public interface IUserClients
    {
        IFeedbacksClient Feedbacks { get; }
        IGroupsClient Groups { get; }
        IGroupsRolesClient GroupsRoles { get; }
        ISprintsClient Sprints { get; }
        ITasksClient Tasks { get; }
        ITokensClient Tokens { get; }
    }
}
