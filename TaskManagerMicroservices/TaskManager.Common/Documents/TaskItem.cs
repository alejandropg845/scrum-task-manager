namespace TaskManager.Common.Documents
{
    public class TaskItem
    {
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string AssignToUsername { get; set; }
        public string TaskTitle { get; set; }
        public string Content { get; set; }
        public bool IsCompleted { get; set; }
        public string GroupName { get; set; }
        public int Priority { get; set; }
    }

}