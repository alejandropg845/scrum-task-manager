using MongoDB.Bson.IO;
using MongoDB.Driver;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using TaskManager.Common.Configurations;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.TaskItems.Clients;
using TaskManager.TaskItems.Interfaces;
using static Google.Apis.Requests.BatchRequest;

namespace TaskManager.TaskItems.Repositories
{
    public class TaskItemsRepository : ITaskItemsWriteRepository, ITaskItemsReadRepository
    {
        private readonly IMongoCollection<TaskItem> _taskItemsCollection;
        private readonly FilterDefinitionBuilder<TaskItem> _filter = Builders<TaskItem>.Filter;
        public TaskItemsRepository(IMongoCollection<TaskItem> collection)
        {
            _taskItemsCollection = collection;
        }

        public async Task<string?> KeepAliveAsync()
        {
            var filter = _filter.Eq(t => t.Content, "");
            var projection = Builders<TaskItem>.Projection.Expression(t => t.Content);

            return await _taskItemsCollection.Find(filter).Project(projection).FirstOrDefaultAsync();

        }
        public async Task<IReadOnlyList<TaskItemDto>> GetTaskItemsAsync(string taskId)
        {
            var filter = _filter.Eq(ti => ti.TaskId, taskId);

            var projection = Builders<TaskItem>.Projection.Expression(ti => new TaskItemDto()
            {
                AssignToUsername = ti.AssignToUsername,
                Content = ti.Content,
                GroupName = ti.GroupName,
                Id = ti.Id,
                Priority = ti.Priority,
                TaskId = ti.TaskId,
                TaskTitle = ti.TaskTitle,
                IsCompleted = ti.IsCompleted
            });

            return await _taskItemsCollection.Find(filter).Project(projection).ToListAsync();
        }

        public async Task<TaskItem> AddTaskItemAsync(CreateTaskItemDto dto)
        {
            var taskItem = new TaskItem
            {
                Id = Guid.NewGuid().ToString(),
                TaskId = dto.TaskId,
                AssignToUsername = dto.AssignToUsername ?? "own task",
                Content = dto.Content.Trim(),
                GroupName = dto.GroupName ?? "no group",
                Priority = 0,
                TaskTitle = dto.TaskTitle
            };

            await _taskItemsCollection.InsertOneAsync(taskItem);

            return taskItem;
        }


        public async Task<bool> TaskItemExistsAsync(string taskItemId)
        => await _taskItemsCollection.Find(_filter.Eq(ti => ti.Id, taskItemId)).AnyAsync();
        public async Task UpdateTaskItemAsync(UpdateTaskItemDto dto)
        {
            var filter = _filter.Eq(ti => ti.Id, dto.TaskItemId);

            var update = Builders<TaskItem>.Update
                .Set(ti => ti.AssignToUsername, dto.AssignTo)
                .Set(ti => ti.Content, dto.Content);

            await _taskItemsCollection.UpdateOneAsync(filter, update);
        }
        public async Task DeleteTaskItemAsync(string taskItemId)
        {
            var filter = _filter.Eq(ti => ti.Id, taskItemId);

            await _taskItemsCollection.DeleteOneAsync(filter);

        }

        public async Task<bool> MarkTaskItemAsCompleted(string taskItemId, string username)
        {
            var filter = _filter.And
            (
                _filter.Eq(ti => ti.Id, taskItemId),
                _filter.Eq(ti => ti.AssignToUsername, username)
            );

            var update = Builders<TaskItem>.Update.Set<bool>(ti => ti.IsCompleted, true);

            await _taskItemsCollection.UpdateOneAsync(filter, update);

            return true;
        }

        public async Task<bool> IsAnyTaskItemNotCompleted(string taskId)
        {
            var filter = _filter.Eq(ti => ti.TaskId, taskId);

            var projection = Builders<TaskItem>.Projection.Expression(t => t.IsCompleted);

            var result = await _taskItemsCollection.Find(filter).Project(projection).ToListAsync();

            return result.Any(ti => !ti);
        }

        public async Task<bool> IsAlreadyCompletedAsync(string taskItemId)
        {
            var filter = _filter.And(
                _filter.Eq(ti => ti.Id, taskItemId),
                _filter.Eq(ti => ti.IsCompleted, true)
            );

            return await _taskItemsCollection.Find(filter).AnyAsync();
        }
        public async Task<IReadOnlyList<TaskItem>> GetUserPendingTaskItemsAsync(string username, string groupName)
        {

            var filter = _filter.And(_filter.Eq(ti => ti.AssignToUsername, username),
                                    _filter.Eq(ti => ti.GroupName, groupName));

            var assignedTasks = await _taskItemsCollection
                .Find(filter).ToListAsync();

            return assignedTasks;
        }

        public async Task<string> DeleteTaskItemsAsync(string taskId) // <== Usado por RabbitMQ
        {   
            var filter = _filter.Eq(ti => ti.TaskId, taskId);

            await _taskItemsCollection.DeleteManyAsync(filter);

            return taskId;
        }
        public async Task<SetPriorityResponse> SetPriorityToTaskItemAsync(SetPriorityToTaskItemDto dto)
        {
            var filter = _filter.And
            (
                _filter.Eq(ti => ti.TaskId, dto.TaskId),
                _filter.Eq(ti => ti.Id, dto.TaskItemId)
            );

            var update = Builders<TaskItem>.Update.Set(ti => ti.Priority, dto.Priority);

            var taskItemPrioritySet = await _taskItemsCollection.UpdateOneAsync(filter, update);

            return new SetPriorityResponse
            {
                Priority = dto.Priority,
                GroupName = dto.GroupName,
                TaskItemId = dto.TaskItemId,
                TaskId = dto.TaskId,
                CanPrioritize = true
            };

        }
    } 
}