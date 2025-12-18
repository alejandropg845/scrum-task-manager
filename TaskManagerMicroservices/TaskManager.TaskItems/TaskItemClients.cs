using TaskManager.TaskItems.Interfaces;

namespace TaskManager.TaskItems
{
    public class TaskItemClients : ITaskItemClients
    {
        public IGeminiClient Gemini { get;  private set; }
        public IGroupsClient Groups { get; private set; }
        public IGroupsRolesClient GroupRoles { get; private set; }
        public ISprintsClient Sprints { get; private set; }
        public ITasksClient Tasks { get; private set; }

        public TaskItemClients(
            IGeminiClient gemini,
            IGroupsClient groups,
            IGroupsRolesClient groupRoles,
            ISprintsClient sprints,
            ITasksClient tasks
        )
        {
            Gemini = gemini;
            Groups = groups;
            GroupRoles = groupRoles;
            Sprints = sprints;
            Tasks = tasks;
        }
    }
}
