namespace TaskManager.TaskItems
{
    public class SetPriorityResponse
    {
       public bool CanPrioritize { get; set; }
       public string TaskId { get; set; }
       public string TaskItemId { get; set; }
       public int Priority { get; set; }
       public string? GroupName { get; set; }
    }

    public class MarkTaskItemAsCompletedResponse
    {
        public bool CanMarkSprintTaskItemAsCompleted { get; set; }
        public bool TaskIsCompleted { get; set; }
        public bool TaskItemExists { get; set; }
        public bool TaskExists { get; set; }
    }
    public class UpdateTaskItemResponse
    {
        public bool TaskItemExists { get; set; }
        public bool TaskExists { get; set; }
        public bool IsTaskOwner { get; set; }
        public bool IsAlreadyCompleted { get; set; }
    }
}
