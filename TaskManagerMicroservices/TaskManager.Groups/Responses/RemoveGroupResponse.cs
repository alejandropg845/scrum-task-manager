namespace TaskManager.Groups.Responses
{
    public class RemoveGroupResponse
    {
        public string DeletedGroup { get; set; }
        public string DeletedGroupOwnerName { get; set; }
        public bool GroupExists { get; set; }
    };
}
