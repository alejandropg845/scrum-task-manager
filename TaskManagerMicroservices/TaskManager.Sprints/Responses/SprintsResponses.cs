namespace TaskManager.Sprints.Responses
{
    public class BeginSprintResponse
    {
        public DateTimeOffset? ExpirationTime { get; set; }
        public List<string> TasksIds { get; set; }
        public string SprintId { get; set; }
        public string? SprintName { get; set; }
        public TimeSpan RemainingTime { get; set; }
    }
}
