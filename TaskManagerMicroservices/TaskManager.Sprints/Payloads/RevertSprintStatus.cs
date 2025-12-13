namespace TaskManager.Sprints.Payloads
{
    public class RevertSprintStatus
    {
        public string SprintId { get; set; }
        public string Status { get; set; }
        public string GroupName { get; set; }
    }
}
