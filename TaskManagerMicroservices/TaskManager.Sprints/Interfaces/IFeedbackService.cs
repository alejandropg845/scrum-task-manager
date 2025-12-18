using MongoDB.Driver;
using TaskManager.Common.Documents;

namespace TaskManager.Sprints.Interfaces
{
    public interface IFeedbackService
    {
        Task<(string? SprintId, bool IsSubmited)> IsFeedbackSubmitedAsync(string username, string groupName, string sprintId);
        Task<bool> DeleteFeedbackAsync(string feedbackId);
        Task AddFeedbackToUsersAsync(string groupName, string sprintId, string token);
        Task MarkFeedbackAsSubmitedAsync(string username, string groupName, string sprintId);
        Task DeleteFeedbacktoUsersAsync(string groupName, string sprintId);
    }
}
