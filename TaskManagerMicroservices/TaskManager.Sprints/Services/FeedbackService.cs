using MongoDB.Driver;
using TaskManager.Common.Documents;
using TaskManager.Sprints.Interfaces;
using TaskManager.Sprints.Repositories;

namespace TaskManager.Sprints.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repo;
        private readonly IUsersClient _usersClient;
        public FeedbackService(IFeedbackRepository repo, IUsersClient usersClient)
        {
            _repo = repo;
            _usersClient = usersClient;
        }

        public async Task<(string? SprintId, bool IsSubmited)> IsFeedbackSubmitedAsync(string username, string groupName, string sprintId)
        => await _repo.IsFeedbackSubmitedAsync(username, groupName, sprintId);

        public async Task<bool> DeleteFeedbackAsync(string feedbackId)
        => await _repo.DeleteFeedbackAsync(feedbackId);

        public async Task MarkFeedbackAsSubmitedAsync(string username, string groupName, string sprintId)
        => await _repo.MarkFeedbackAsSubmitedAsync(username, groupName, sprintId, null);
        public async Task AddFeedbackToUsersAsync(string groupName, string sprintId, string token)
        {
            var users = await _usersClient.GetUsersAsync(groupName, token);

            var developerUsers = users.Where(u => u.GroupRole == "developer");

            var tasks = new List<Task>();

            foreach (var user in developerUsers)
            {
                var task = _repo.AddFeedbackAsync
                (
                    groupName,
                    user.Username,
                    sprintId
                );

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }
        public async Task DeleteFeedbacktoUsersAsync(string groupName, string sprintId)
        => await _repo.DeleteFeedbackToUsersAsync(groupName, sprintId);
    }
}
