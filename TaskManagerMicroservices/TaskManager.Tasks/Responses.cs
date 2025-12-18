using TaskManager.Common.Documents;

namespace TaskManager.Tasks
{
    public class DeleteTaskResponse
    {
        public UserTask? DeletedTask { get; set; }
        public bool TaskExists { get; set; }
        public bool TaskCanBeDeleted { get; set; }
    }
}
