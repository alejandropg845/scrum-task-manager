using MongoDB.Driver;
using TaskManager.Common.Documents;

namespace TaskManager.Sprints.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<(string? SprintId, bool IsSubmited)> IsFeedbackSubmitedAsync(string username, string groupName, string sprintId);
        Task<Feedback> AddFeedbackAsync(string groupName, string username, string sprintId);
        Task<bool> DeleteFeedbackAsync(string feedbackId);
        Task MarkFeedbackAsSubmitedAsync(string username, string groupName, string sprintId, IClientSessionHandle? transaction);
        Task DeleteFeedbackToUsersAsync(string groupName, string sprintId);
    }
}
