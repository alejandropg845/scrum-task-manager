using TaskManager.Common.DTOs;

namespace TaskManager.Users.Interfaces.Clients
{
    public interface IFeedbacksClient
    {
        Task<FeedbackDto> IsFeedbackSubmitedAsync(string groupName, string sprintId, string token);
        Task AddFeedbackToUsersAsync(string groupName, string sprintId, string token);
    }
}
