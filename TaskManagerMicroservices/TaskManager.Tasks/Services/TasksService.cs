using System.Text.Json;
using System.Threading.Tasks;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Mappers;
using TaskManager.Tasks.DTOs;
using TaskManager.Tasks.Interfaces;

namespace TaskManager.Tasks.Services
{
    public class TasksService : ITaskWriteService, ITaskReadService
    {
        private readonly ITaskWriteRepository _repoWrite;
        private readonly ITaskReadRepository _repoRead;
        private readonly IMessageBusClient _messageBus;

        private readonly ITaskClients _tc;
        public TasksService(ITaskWriteRepository tasksRepository, 
            IMessageBusClient mbc, 
            ITaskReadRepository repoRead, 
            ITaskClients tc)
        {
            _repoWrite = tasksRepository;
            _messageBus = mbc;
            _repoRead = repoRead;
            _tc = tc;

        }

        public async Task<IReadOnlyList<UserTaskDto>> GetUserTasksAsync(string username, string groupName, string token)
        {
            IReadOnlyList<UserTask> tasks = [];

            if (GroupNameIsNull(groupName))
                tasks = await _repoRead.GetUserTasksAsync(username);
            else
                tasks = await _repoRead.GetGroupTasksAsync(groupName);


            if (tasks.Count > 0)
            {
                /* Listado de tareas (NO DE LA APLICACIÓN) para procesar */
                var taskItems_Tasks = new List<Task<JsonDocument?>>();

                /* Por cada userTask guardamos un Task de JsonDocument donde cada Json es un listado de taskItems
                 * (NO DE LA APLICACIÓN) para obtener sus items */
                foreach (var task in tasks)
                {
                    var notResolvedTask = _tc.TaskItems.GetTaskItemsAsync(task.Id, token);
                    taskItems_Tasks.Add(notResolvedTask);
                }

                /* Ejecutar listado de tareas a procesar*/
                await Task.WhenAll(taskItems_Tasks);

                var mixedTasksItems = new List<TaskItem>();

                /* Obtenemos un listado de lista de tareas, donde cada indice de la lista es un listado de tareas */

                var jsonSerializerOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                foreach (var taskItemsList_task in taskItems_Tasks)
                {
                    /* Marcamos con ? porque peude que el UserTask no tenga taskITems */
                    var taskItems = (await taskItemsList_task)?.Deserialize<List<TaskItem>>(jsonSerializerOptions);
                    mixedTasksItems.AddRange(taskItems ?? []);
                }

                foreach (var task in tasks) task.TaskItems = mixedTasksItems
                        .Where(ti => ti.TaskId == task.Id).ToList();

            }

            return tasks.Select(ut => ut.ToUserTaskDto(username)).ToList();
        }
        public async Task<(UserTask? userTask, string ErrorMessage)> AddUserTaskAsync(string username, CreateTaskDto dto, string token)
        {

            if (!GroupNameIsNull(dto.GroupName))
            {

                bool isAddingTasksAllowed = await _tc.Groups.IsAddingTasksAllowed(dto.GroupName!, token);

                if (!isAddingTasksAllowed)
                {
                    bool isGroupOwner = await _tc.Groups.IsUserGroupOwnerAsync(dto.GroupName!, username, token);

                    if (!isGroupOwner) return new(null, "Task adding is restricted to group owner");
                }

            }

            if (GroupNameIsNull(dto.GroupName))
                dto.GroupName = "no group";

            var task = new UserTask
            {
                CreatedOn = DateTimeOffset.UtcNow,
                TaskItems = [],
                Username = username,
                Id = Guid.NewGuid().ToString(),
                Title = dto.Title,
                GroupName = dto.GroupName!,
                Priority = 0
            };

            var createdTask = await _repoWrite.AddTaskAsync(task);

            return new(createdTask, string.Empty);
        }
        public async Task<DeleteTaskResponse> DeleteTaskAsync(string? groupName, string taskId, string token, string username)
        {
            var response = new DeleteTaskResponse();

            //Validar que exista la tarea a borrar
            bool taskExists = await _repoRead.TaskExistsAsync(taskId);

            response.TaskExists = taskExists;

            if (!taskExists) return response;

            if (!GroupNameIsNull(groupName))
            {
                bool isScrum = await _tc.Groups.IsScrumAsync(groupName!, token);

                if (isScrum)
                {

                    var isTaskOwner_task = _repoRead.IsTaskOwnerAsync(taskId, username);
                    var roleName_task = _tc.GroupRoles.GetRoleNameAsync(groupName!, username, token);

                    await Task.WhenAll(isTaskOwner_task, roleName_task);

                    /* Si la tarea a eliminar es del que la creó o si es Product Owner, se puede eliminar */
                    if (await isTaskOwner_task || await roleName_task == "product owner")
                    {
                        var deletedTaskk = await _repoWrite.DeleteTaskWithoutScrumAsync(taskId);
                        _messageBus.Publish("delete_task_items", taskId);
                        response.DeletedTask = deletedTaskk;
                        response.TaskCanBeDeleted = true;

                        return response;
                    }

                    /* No puede ser eliminada */
                    response.TaskCanBeDeleted = false;
                    return response;


                }

                /* No es scrum */

                var isTaskOwner_T = _repoRead.IsTaskOwnerAsync(taskId, username);

                var isGroupOwner_T = _tc.Groups.IsUserGroupOwnerAsync(groupName!, username, token);

                await Task.WhenAll(isTaskOwner_T, isGroupOwner_T);

                bool isTaskOwner = await isTaskOwner_T;
                bool isGroupOwner = await isGroupOwner_T;

                bool canDeleteGroupTask = (isTaskOwner || isGroupOwner);

                response.TaskCanBeDeleted = canDeleteGroupTask;

                if (!canDeleteGroupTask) return response;

                var deletedTaskkk = await _repoWrite.DeleteTaskWithoutScrumAsync(taskId);
                _messageBus.Publish("delete_task_items", taskId);

                response.DeletedTask = deletedTaskkk;

                return response;
            }

            /* No hay grupo */

            var deletedTask = await _repoWrite.DeleteTaskWithoutScrumAsync(taskId);
            _messageBus.Publish("delete_task_items", taskId);
            response.DeletedTask = deletedTask;
            response.TaskCanBeDeleted = true;

            return response;

        }
        private static bool GroupNameIsNull(string? groupName)
        {
            if (string.IsNullOrEmpty(groupName) || groupName == "null" || groupName is "")
                return true;

            return false;
        }

        public async Task<List<string>> SetSprintToTasksAsync(SprintInfoForTask info)
        => await _repoWrite.SetSprintToTasksAsync(info);

        public async Task<bool> SetTaskAsCompletedAsync(string taskId)
        => await _repoWrite.SetTaskAsCompletedAsync(taskId);

        public async Task MarkSprintTasksAsFinishedAsync(string sprintId)
        => await _repoWrite.MarkSprintTasksAsFinishedAsync(sprintId);

        public async Task<bool> TaskContainsSprintAsync(string taskId)
        => await _repoRead.TaskContainsSprintAsync(taskId);

        public async Task<bool> SetTaskPriorityAsync(PrioritizeTaskDto dto, string username, string token)
        {
            var roleName = await _tc.GroupRoles.GetRoleNameAsync(dto.GroupName, username, token);

            bool canPrioritize = roleName is "product owner" || roleName is "scrum master";

            if (!canPrioritize) return false;

            await _repoWrite.SetTaskPriorityAsync(dto.TaskId, username, dto.Priority);

            return true;
        }

        public async Task RevertSprintTasksSetAsFinishedAsync(string sprintId)
        => await _repoWrite.RevertSprintTasksSetAsFinishedAsync(sprintId);

        public async Task<string?> GetTaskOwnerNameAsync(string taskItemId)
        => await _repoRead.GetTaskOwnerNameAsync(taskItemId);

        public async Task<bool> TaskExistsAsync(string taskId)
        => await _repoRead.TaskExistsAsync(taskId);
        
        public async Task<bool> IsTaskOwnerAsync(string taskId, string username)
        => await _repoRead.IsTaskOwnerAsync(taskId, username);
        public async Task<List<UserTaskDto>> GetCompletedTasksAsync(string token, string groupName)
        {
            var tasks = await _repoRead.GetCompletedTasksAsync(groupName);

            /* Listado de tareas (NO DE LA APLICACIÓN) para procesar */
            var taskItems_Tasks = new List<Task<JsonDocument?>>();

            /* Por cada userTask guardamos un Task de JsonDocument donde cada Json es un listado de taskItems
             * (NO DE LA APLICACIÓN) para obtener sus items */
            foreach (var task in tasks)
            {
                var notResolvedTask = _tc.TaskItems.GetTaskItemsAsync(task.Id, token);
                taskItems_Tasks.Add(notResolvedTask);
            }

            /* Ejecutar listado de tareas a procesar*/
            await Task.WhenAll(taskItems_Tasks);

            var mixedTasksItems = new List<TaskItemDto>();

            /* Obtenemos un listado de lista de tareas, donde cada indice de la lista es un listado de tareas */

            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            foreach (var taskItemsList_task in taskItems_Tasks)
            {
                
                var taskItems = (await taskItemsList_task)!.Deserialize<List<TaskItemDto>>(jsonSerializerOptions)!;
                mixedTasksItems.AddRange(taskItems);
            }

            foreach (var task in tasks) task.TaskItems = mixedTasksItems
                    .Where(ti => ti.TaskId == task.Id).ToList();

            return tasks;
        }
    }
}
