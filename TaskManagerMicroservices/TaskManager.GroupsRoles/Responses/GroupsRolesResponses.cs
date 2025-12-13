using TaskManager.Common.Documents;

namespace TaskManager.GroupsRoles.Responses
{
    public class SetGroupRoleResponse
    {
        public Common.Documents.GroupsRoles GroupRole { get; set; }
        public bool IsProductOwner { get; set; }
        public bool IsChangingOwnRole { get; set; }
        public bool IsSwitchingScrumMaster { get; set; }
        public string? UserThatAssignedProductOwner { get; set; }
        public string? UserThatWasScrumMaster { get; set; }
        public string? UserThatIsScrumMaster { get; set; }
        public bool IsTransactionError { get; set; }
    }
}
