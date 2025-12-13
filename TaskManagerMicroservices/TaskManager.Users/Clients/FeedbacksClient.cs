using System.Text.Json;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.Users.Interfaces.Clients;

namespace TaskManager.Users.Clients
{
    public class FeedbacksClient : IFeedbacksClient
    {
        private readonly IAuthenticationClient _authenticationClient;
        public FeedbacksClient(IAuthenticationClient c)
        {
            _authenticationClient = c;
        }

        public async Task<FeedbackDto> IsFeedbackSubmitedAsync(string groupName, string sprintId, string token)
        {
            var isFeedbackSubmited_json = await _authenticationClient
            .SendRequestAsync(
                "sprints",
                $"feedbacks/IsFeedbackSubmited/{groupName}?sprintId={sprintId}",
                "get",
                token,
                null
            );

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            return isFeedbackSubmited_json!.Deserialize<FeedbackDto>(jsonOptions)!;
        }

        public Task AddFeedbackToUsersAsync(string groupName, string sprintId, string token)
        {
            return _authenticationClient
            .SendRequestAsync(
                "sprints",
                $"feedbacks/AddFeedbackToDevelopers/{groupName}?sprintId={sprintId}",
                "post",
                token,
                null
            );
        }
    }
}
