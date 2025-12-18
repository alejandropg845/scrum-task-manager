using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.Chats.Interfaces;
using TaskManager.Common.DTOs;

namespace TaskManager.Chats.Controllers
{
    [ApiController]
    [Route("api/chats")]
    public class ChatController : ControllerBase
    {
        private readonly IDatesService _datesService;
        public ChatController(IDatesService datesService)
        {
            _datesService = datesService;
        }

        [HttpGet("GetGroupChatMessages")]
        [Authorize]
        public async Task<ActionResult> GetGroupChatMessages([FromBody] GetMessagesDto dto)
        {
            var response = await _datesService.GetMessagesAsync(dto);

            if (response.NoMoreDates) return NotFound(new { Message = "No more messages" });

            return Ok(new
            {
                response.MessagesDate,
                response.Messages,
                response.DateId,
                response.NoMoreMessages
            });


        }

        [HttpPost("SendMessage")]
        [Authorize]
        public async Task<ActionResult> SendMessageAsync([FromBody] SendMessageDto dto)
        {
            string? username = User.FindFirstValue(ClaimTypes.Name);

            if (username is null) return Unauthorized();

            var result = await _datesService.SendMessageAsync(dto, username);

            if (result.IsTransactionError) return StatusCode(500);

            return Ok(new
            {
                result.Message,
                result.MessagesDate
            });
        }
    }
}
