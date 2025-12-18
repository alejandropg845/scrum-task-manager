namespace TaskManager.TaskItems.Interfaces
{
    public interface ISprintsClient
    {
        Task<bool> CanMarkTaskItemAsCompletedAsync(string sprintId, string token);
    }
}
