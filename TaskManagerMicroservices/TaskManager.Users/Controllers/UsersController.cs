using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;
using TaskManager.Common.Interfaces;
using TaskManager.Common.DTOs;
using TaskManager.Common.Configurations;
using TaskManager.Users.Interfaces.Service;

namespace TaskManager.Users.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserAuthService _usersAuthService;
        private readonly IUserManagementService _usersManageService;
        private readonly IUserInfoService _userInfoService;
        public UsersController(IUserAuthService service, IUserManagementService manageService, IUserInfoService infoService)
        {
            _usersAuthService = service;
            _usersManageService = manageService;
            _userInfoService = infoService;
        }

        [HttpGet("GetUsers/{groupName}")]
        [Authorize]
        public async Task<ActionResult> GetUsers([FromRoute] string groupName)
        {

            bool groupNameHasValue = !string.IsNullOrEmpty(groupName) && groupName is not "null" && groupName is not "undefined";

            if(groupNameHasValue)
            {
                string token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

                return Ok(await _usersAuthService.GetUsersAsync(groupName, token));
            }
            return BadRequest();
        }

        [HttpPost("RegisterUser")]
        public async Task<ActionResult> RegisterUser([FromBody] RegisterUserDto dto)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);


            var r = await _usersAuthService.RegisterUserAsync(dto);

            if(r.UserExists) return BadRequest(new { Message = "Username already taken" });

            if (r.EmailExists) return BadRequest(new { Message = "Email already exists" });

            return Ok
            (
                new 
                {
                    Ok = true,
                    r.AccessToken,
                    r.RefreshToken,
                    Message = "Logged in"
                }
            );
        }

        [HttpPost("LoginUser")]
        public async Task<ActionResult> LoginUser([FromBody] LoginUserDto dto)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);


            var r = await _usersAuthService.LoginUserAsync(dto);

            if(!r.IsCorrect || r.UserDoesntExist) 
            return BadRequest(new { Message = "Incorrect credentials" });

            return Ok
            (
                new
                {
                    Ok = true,
                    r.AccessToken,
                    r.RefreshToken,
                    Message = "Logged in"
                }
            );
        }

        [HttpPost("ContinueWithGoogle/{tokenId}")]
        public async Task<ActionResult> ContinueWithGoogle([FromRoute] string tokenId)
        {
            var r = await _usersAuthService.ContinueWithGoogleAsync(tokenId);

            if (r.IsGoogleAuthError) return Unauthorized();

            return Ok
            (
                new
                {
                    Ok = true,
                    r.AccessToken,
                    r.RefreshToken,
                    Message = "Logged in"
                }
            );


        }
        [HttpGet("GetUserGroupName")]
        [Authorize]
        public async Task<ActionResult> GetUserGroupName()
        {
            string? username = User.FindFirstValue(ClaimTypes.Name);

            if (username is null) return Unauthorized();

            string? result = await _userInfoService.GetUserGroupNameAsync(username);

            return Ok(new { result });

        }

        [HttpGet("GetUserInfo")]
        [Authorize]
        public async Task<ActionResult> GetUserInfo()
        {

            string? username = User.FindFirstValue(ClaimTypes.Name);

            if (username is not null)
            {

                string token = HttpContext.Request.Headers.Authorization.ToString().Replace("bearer ", "");

                var r = await _userInfoService.GetUserInfoAsync(username, token);

                if (r.IsError) return StatusCode(500);

                return Ok(
                    new {
                        r.GroupName,
                        r.IsGroupOwner,
                        Username = username,
                        r.GroupRole,
                        r.IsScrum,
                        IsAddingTasksAllowed = r.IsAllowed,
                        r.ExpirationTime,
                        r.Status,
                        r.SprintNumber,
                        r.AvatarBgColor,
                        r.SprintName,
                        r.RemainingTime,
                        r.FinishedSprintName,
                        r.FinishedSprintId
                    }
                );
            }

            return Unauthorized();
        }

        [HttpPost("SetGroupToUser/{groupName}")]
        [Authorize]
        public async Task<ActionResult> SetGroupToUser([FromRoute] string groupName)
        {

            string? username = User.FindFirstValue(ClaimTypes.Name);

            if (username is null) return Unauthorized();

            await _usersManageService.SetGroupToUserAsync(username, groupName);

            return NoContent();

        }

        [HttpPut("LeaveGroup/{groupName}")]
        [Authorize]
        public async Task<ActionResult> LeaveGroup([FromRoute] string groupName)
        {

            string? username = User.FindFirstValue(ClaimTypes.Name);

            if (username is not null)
            {

                await _usersManageService.LeaveGroupAsync(username, groupName);

                return NoContent();
            }

            return Unauthorized();
        }

        [HttpPost("RecoverPassword/{email}")]
        public async Task<ActionResult> RecoverPassword([FromRoute] string email)
        {
            await _usersAuthService.RecoverPasswordAsync(email);

            return NoContent();

        }

        [HttpPost("ReceiveRecoveryCode")]
        public async Task<ActionResult> ReceiveRecoveryCodeAsync([FromBody] ReceiveRecoveryCodeDto dto)
        {
            
            var r = await _usersAuthService.ReceiveRecoveryCodeAsync(dto.RecoveryCode, dto.Email, dto.Password1, dto.Password2);

            if (!r.PasswordsMatch) return BadRequest(new { Message = "Passwords don't match" });
            
            if (r.IsExpired) return BadRequest(new { Message = "This attempt has expired. Start the process again", Restart = true });

            if (!r.RecoveryCodeIsOk) return BadRequest(new { Message = "Recovery code doesn't match" });


            return Ok(new { Message = "Password changed successfully!" });

        }
    }
}