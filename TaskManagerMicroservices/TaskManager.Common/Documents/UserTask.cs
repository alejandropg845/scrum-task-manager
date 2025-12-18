using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TaskManager.Common.Documents
{
    public class UserTask
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string GroupName { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string SprintStatus { get; set; }
        public string? SprintId { get; set; }
        public int Priority { get; set; }
        public IReadOnlyCollection<TaskItem> TaskItems { get; set; }
    }

}