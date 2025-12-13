
using System.Text.Json;

namespace TaskManager.Groups.Interfaces
{
    public interface IGroupWriteRepository
    {
        Task<bool> SetAddingTasksAllowed(string username, string groupName, bool isAllowed);
        Task<bool> DeleteGroupSprintAsync(string groupName, string sprintId);
        Task<bool> DeleteGroupAsync(string groupName, string username);
        Task<JsonDocument?> SetSprintIdToGroupAsync(string groupName, string sprintId);
        Task DeleteCreatedGroupAsync(string groupName);
        Task<JsonDocument?> AddGroupAsync(string groupName, string username, bool isScrum);
    }
}
