using TaskManager.Tasks.Interfaces;

namespace TaskManager.Tasks
{
    public class TaskClients : ITaskClients
    {
        public IGroupsClient Groups { get; private set; }

        public IGroupsRolesClient GroupRoles {get;private set;}

        public ITaskItemsClient TaskItems {get;private set;}
        public TaskClients(IGroupsClient groupsClient, IGroupsRolesClient groupRolesClient, ITaskItemsClient taskItemClient)
        {
            Groups = groupsClient;
            GroupRoles = groupRolesClient;
            TaskItems = taskItemClient;
        }
    }
}
