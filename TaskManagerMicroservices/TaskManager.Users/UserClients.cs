using TaskManager.Common.Documents;
using TaskManager.Users.Interfaces;
using TaskManager.Users.Interfaces.Clients;

namespace TaskManager.Users
{
    public class UserClients : IUserClients
    {
        public IFeedbacksClient Feedbacks { get; private set; }
        public IGroupsClient Groups { get; private set; }
        public IGroupsRolesClient GroupsRoles { get; private set; }
        public ISprintsClient Sprints { get; private set; }
        public ITasksClient Tasks { get; private set; }
        public ITokensClient Tokens { get; private set; }

        public UserClients(
        IFeedbacksClient feedbacks,
        IGroupsClient groups,
        IGroupsRolesClient groupsRoles,
        ISprintsClient sprints,
        ITasksClient tasks,
        ITokensClient tokens)
        {
            Feedbacks = feedbacks;
            Groups = groups;
            GroupsRoles = groupsRoles;
            Sprints = sprints;
            Tasks = tasks;
            Tokens = tokens;
        }
    }
}
