using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;

namespace TaskManager.Sprints.Interfaces
{
    public interface ITasksClient
    {
        Task<List<UserTaskDto>> GetSprintsTasksAsync(string token, string groupName);
    }
}
