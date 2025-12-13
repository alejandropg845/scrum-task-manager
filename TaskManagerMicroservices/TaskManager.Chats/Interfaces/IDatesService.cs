using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;

namespace TaskManager.Chats.Interfaces
{
    public interface IDatesService
    {
        Task<GetMessagesResponse> GetMessagesAsync(GetMessagesDto dto);
        Task<SendMessageResponse> SendMessageAsync(SendMessageDto dto, string username);

    }
}
