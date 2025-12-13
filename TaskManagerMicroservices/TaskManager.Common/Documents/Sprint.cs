
using TaskManager.Common.DTOs;

namespace TaskManager.Common.Documents
{
    public class Sprint
    {
        public string Id { get;set; }
        public string GroupName { get;set; }
        public string Status { get; set; }
        public int SprintNumber { get; set; }
        public string? SprintName { get; set; }
        public DateTimeOffset? SprintExpiration { get; set; }
    }
}
