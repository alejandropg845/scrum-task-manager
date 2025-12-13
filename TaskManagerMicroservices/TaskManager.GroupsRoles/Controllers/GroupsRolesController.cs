using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.Common.DTOs;
using TaskManager.GroupsRoles.Interfaces;

namespace TaskManager.GroupsRoles.Controllers
{
    [ApiController]
    [Route("api/groupsRoles")]
    public class GroupsRolesController : ControllerBase
    {
        private readonly IGroupsRolesService _service;
        public GroupsRolesController(IGroupsRolesService service)
        {
            _service = service;
        }

        [HttpGet("GetUsersGroupRoles/{groupName}")]
        [Authorize]
        public async Task<ActionResult> GetUsersGroupRoles([FromRoute] string groupName)
        {
            
            var result = await _service.GetUsersGroupRolesAsync(groupName);
            return Ok(result);
           
        }

        [HttpGet("GetUserGroupRole/{groupName}")]
        [Authorize]
        public async Task<ActionResult> GetUserGroupRole([FromRoute] string groupName)
        {

            string? username = User.FindFirstValue(ClaimTypes.Name);


            if(username is not null)
            {
                string? result = await _service.GetGroupRoleNameAsync(groupName, username);

                return Ok(new { result });
            }

            return Unauthorized();
        }

        [HttpPut("SetUserGroupRole/{username}")]
        [Authorize]
        public async Task<ActionResult> SetUserGroupRole([FromRoute] string username, [FromBody] SetUserGroupRoleDto dto)
        {

            string? currentUsername = User.FindFirstValue(ClaimTypes.Name);

            if (currentUsername is null) return Unauthorized();

            var response = await _service.SetGroupRoleAsync(dto, username, currentUsername);


            if (!response.IsProductOwner) return BadRequest(new
            {
                Message = "You're not allowed to set user roles",
                response.IsProductOwner
            });

            if (response.IsChangingOwnRole) return BadRequest(new
            {
                Message = "The Product Owner cannot change their own role"
            });

            if (response.IsTransactionError) return StatusCode(500);

            return Ok(new
            {
                response.GroupRole,
                response.UserThatAssignedProductOwner,
                response.IsSwitchingScrumMaster,
                response.UserThatWasScrumMaster,
                response.UserThatIsScrumMaster
            });
            
        }

        [HttpDelete("DeleteGroupRoles")]
        [Authorize]
        public async Task<ActionResult> DeleteGroupRoles([FromHeader] string groupName)
        {
            string result = await _service.RemoveGroupsRolesAsync(groupName);

            return Ok(new { result });
        }
      
    }
}
