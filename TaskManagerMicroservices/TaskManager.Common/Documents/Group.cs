using System.Text.RegularExpressions;

namespace TaskManager.Common.Documents
{
    public class Group
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string OwnerName { get; set; } 
        public bool IsScrum { get; set; }
        public bool IsAddingTasksAllowed { get; set; }
        public string SprintId { get; set; }
    }

}

