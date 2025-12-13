using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Common.DTOs
{
    public record SetUserGroupRoleDto
    (
      [Required] string GroupName,
      [Required] string RoleName
    );

    public record SetUserGroupRoleHubDto
    (
        [Required] string GroupName,
        [Required] string Username,
        [Required] string GroupRole,
        string? UserThatAssignedProductOwner,
        bool IsSwitchingScrumMaster,
        string? UserThatWasScrumMaster,
        string? UserThatIsScrumMaster
    );
    public record GroupRoleDto(string Username, string? RoleName);
}
