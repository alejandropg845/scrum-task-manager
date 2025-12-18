using MongoDB.Driver;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Tasks.Interfaces;

namespace TaskManager.Tasks.Repositories
{
    public class TasksRepository : ITaskWriteRepository, ITaskReadRepository
    {
        private readonly IMongoCollection<UserTask> _tasksCollection;
        private readonly FilterDefinitionBuilder<UserTask> _fb = Builders<UserTask>.Filter;
        public TasksRepository(IMongoCollection<UserTask> collection)
        {
            _tasksCollection = collection;
        }
        public async Task<string?> KeepAliveAsync()
        {
            var filter = _fb.Eq(t => t.Title, "anyTask#423454");
            var projection = Builders<UserTask>.Projection.Expression(t => t.Title);

            return await _tasksCollection.Find(filter).Project(projection).FirstOrDefaultAsync();
        }
        public async Task<IReadOnlyList<UserTask>> GetGroupTasksAsync(string groupName)
        {
            var filter = _fb.Eq(ut => ut.GroupName, groupName);
            return await _tasksCollection.Find(filter).ToListAsync();
        }

        public async Task<IReadOnlyList<UserTask>> GetUserTasksAsync(string username)
        {
            var filter = _fb.And
            (
                _fb.Eq(ut => ut.Username, username),
                _fb.Eq(ut => ut.GroupName, "no group")
            );

            return await _tasksCollection.Find(filter).ToListAsync();
        }
        public async Task<UserTask> AddTaskAsync(UserTask task)
        {
            await _tasksCollection.InsertOneAsync(task);

            return task;
        }
        public async Task<bool> TaskExistsAsync(string taskId)
        {
            var filter = _fb.Eq(t => t.Id, taskId);

            return await _tasksCollection.Find(filter).AnyAsync();
        }
        public async Task<UserTask> DeleteTaskWithoutScrumAsync(string taskId)
        {
            var filter = _fb.And(
                _fb.Eq(t => t.Id, taskId),
                _fb.Eq(t => t.SprintId, null)
            );

            var deletedTask = await _tasksCollection.FindOneAndDeleteAsync(filter);
            
            return deletedTask;
        }
        public async Task<bool> IsTaskOwnerAsync(string taskId, string username)
        {
            var filter = _fb.And(
                _fb.Eq(t => t.Id, taskId),
                _fb.Eq(t => t.Username, username)
            );

            return await _tasksCollection.Find(filter).AnyAsync();
        }

        public async Task<string?> GetTaskOwnerNameAsync(string taskId)
        {
            var projection = Builders<UserTask>.Projection.Expression
                (t => t.Username);

            var filter = _fb.Eq(t => t.Id, taskId);

            return await _tasksCollection.Find(filter).Project(projection).FirstOrDefaultAsync();
        }

        public async Task<List<string>> SetSprintToTasksAsync(SprintInfoForTask info)
        {
            var update = Builders<UserTask>.Update
                .Set(t => t.Status, "in progress")
                .Set(t => t.SprintId, info.SprintId);

            var tasks = new List<Task>();

            foreach (var taskId in info.TasksIds)
            {
                var filter = _fb.Eq(t => t.Id, taskId);
                tasks.Add(_tasksCollection.UpdateOneAsync(filter, update));
            }

            await Task.WhenAll(tasks);

            return info.TasksIds;
        }

        public async Task RevertInProgressStatusAsync(string sprintId)
        {
            if (sprintId is null) return;

            var filter = _fb.Eq(t => t.SprintId, sprintId);

            var update = Builders<UserTask>.Update
                .Set(t => t.Status, string.Empty)
                .Set(t => t.SprintId, null);

            await _tasksCollection.UpdateManyAsync(filter, update);
        }

        public async Task<bool> SetTaskAsCompletedAsync(string taskId)
        {
            var filter = _fb.Eq(t => t.Id, taskId);

            var update = Builders<UserTask>.Update.Set(t => t.Status, "completed");

            await _tasksCollection.UpdateOneAsync(filter, update);

            return true;
        }

        public async Task MarkSprintTasksAsFinishedAsync(string sprintId)
        {
            var filter = _fb.Eq(t => t.SprintId, sprintId);

            var update = Builders<UserTask>.Update
                .Set(t => t.SprintStatus, "finished");

            await _tasksCollection.UpdateManyAsync(filter, update);

        }

        public async Task<bool> TaskContainsSprintAsync(string taskId)
        {
            var filter = _fb.And(
                _fb.Eq(t => t.Id, taskId),
                _fb.Ne(t => t.SprintId, null)
            );

            return await _tasksCollection.Find(filter).AnyAsync();
        }
        public async Task<bool> SetTaskPriorityAsync(string taskId, string username, int priority)
        {
            var filter = _fb.And(
                _fb.Eq(t => t.Id, taskId),
                _fb.Eq(t => t.Username, username)
            );

            var update = Builders<UserTask>.Update.Set(t => t.Priority, priority);

            await _tasksCollection.UpdateOneAsync(filter, update);

            return true;
        }

        public async Task<List<UserTaskDto>> GetCompletedTasksAsync(string groupName)
        {
            var filter = _fb.And(
                _fb.Eq(t => t.GroupName, groupName),
                _fb.Eq(t => t.SprintStatus, "finished"),
                _fb.Ne(t => t.SprintId, null)
            );

            var projection = Builders<UserTask>.Projection.Expression(u => new UserTaskDto
            {
                CreatedOn = u.CreatedOn,
                GroupName = u.GroupName,
                Id = u.Id,
                Priority = u.Priority,
                SprintId = u.SprintId,
                SprintStatus = u.SprintStatus,
                Status = u.Status,
                Title = u.Title,
                Username = u.Username
            });

            return await _tasksCollection.Find(filter).Project(projection).ToListAsync();

        }
        public async Task RevertSprintTasksSetAsFinishedAsync(string sprintId)
        {
            var filter = _fb.Eq(t => t.SprintId, sprintId);

            var update = Builders<UserTask>.Update.Set(t => t.SprintStatus, null);

            await _tasksCollection.UpdateManyAsync(filter, update);

        }

    }
}