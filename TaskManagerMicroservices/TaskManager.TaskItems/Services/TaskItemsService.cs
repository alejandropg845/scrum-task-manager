using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.TaskItems.Interfaces;

namespace TaskManager.TaskItems.Services
{
    public class TaskItemsService : ITaskItemsWriteService, ITaskItemsReadService
    {
        private readonly ITaskItemsWriteRepository _repoWrite;
        private readonly ITaskItemsReadRepository _repoRead;
        private readonly IMessageBusClient _messageBusClient;
        private readonly ITaskItemClients _tic;
        public TaskItemsService(ITaskItemsWriteRepository repo, IMessageBusClient mb, ITaskItemsReadRepository repoRead, ITaskItemClients ti)
        {
            _repoWrite = repo;
            _repoRead = repoRead;
            _messageBusClient = mb;
            _tic = ti;
        }
        public async Task<IReadOnlyList<TaskItem>> GetUserPendingTaskItemsAsync(string username, string groupName)
        => await _repoRead.GetUserPendingTaskItemsAsync(username, groupName);
        public async Task<(TaskItem ti, bool AssignToUserError, bool ContainsScrum)> CreateTaskItemAsync(CreateTaskItemDto dto, string username, string token)
        {
            if (dto.GroupName is not null)
            {
                bool isScrum = await _tic.Groups.IsScrumAsync(dto.GroupName, token);

                if (isScrum)
                {
                    /*Si sprintId no es null, quiere decir que hay un sprint en progreso o ya terminó
                     por lo que no se puede agregar el taskItem en el Task*/
                    bool taskContainsSprint = await _tic.Tasks
                        .TaskContainsSprintAsync(dto.TaskId, token);

                    if (taskContainsSprint)
                        return new(new TaskItem(), false, true);
                }
            }

            var createdTaskItem = await _repoWrite.AddTaskItemAsync(dto);

            return new(createdTaskItem, false, false);
        }
        public async Task<bool> DeleteTaskItemAsync(DeleteTaskItemDto dto, string token)
        {
            bool groupIsNotNull = dto.GroupName is not "null" 
                                    && dto.GroupName is not "undefined" 
                                    && !string.IsNullOrEmpty(dto.GroupName);

            if (groupIsNotNull)
            {

                bool isScrum = await _tic.Groups.IsScrumAsync(dto.GroupName!, token);

                if (isScrum)
                {
                    /*Si sprintId no es null, quiere decir que hay un sprint en progreso o ya terminó
                     por lo que no se puede agregar el taskItem en el Task*/

                    bool taskContainsSprint = await _tic.Tasks.TaskContainsSprintAsync(dto.TaskId, token);

                    if (taskContainsSprint)

                        return false;
                }
            }

            await _repoWrite.DeleteTaskItemAsync(dto.TaskItemId);

            return true;
        }

        public async Task<IReadOnlyList<TaskItemDto>> GetTaskItemsAsync(string taskId)
        => await _repoRead.GetTaskItemsAsync(taskId);

        public async Task<SetPriorityResponse> SetPriorityToTaskItemAsync(SetPriorityToTaskItemDto dto, string username, string token)
        {
            var response = new SetPriorityResponse();

            if (dto.GroupName is not null && dto.GroupName is not "no group")
            {

                bool isScrum = await _tic.Groups.IsScrumAsync(dto.GroupName, token);

                if (isScrum)
                {
                    /* Validar que el usuario modificando es el product owner */
                    string? roleName = await _tic.GroupRoles.GetUserGroupRoleAsync(dto.GroupName, token);

                    bool canPrioritize = (roleName == "product owner" || roleName == "scrum master");

                    response.CanPrioritize = canPrioritize;
                    if (!canPrioritize) return response;
                }
            }

            response = await _repoWrite.SetPriorityToTaskItemAsync(dto);
            
            return response;
        }

        public async Task<MarkTaskItemAsCompletedResponse> SetTaskItemAsCompletedAsync(string username, MarkTaskItemAsCompletedDto dto, string token)
        {
            var response = new MarkTaskItemAsCompletedResponse();

            response.TaskItemExists = await _repoRead.TaskItemExistsAsync(dto.TaskItemId);

            if (!response.TaskItemExists) return response;

            response.TaskExists = await _tic.Tasks.TaskExistsAsync(dto.TaskId, token);

            if (!response.TaskExists) return response;

            bool isScrum = await _tic.Groups.IsScrumAsync(dto.GroupName, token);

            if (isScrum)
            {
                /* Verificamos que el SprintId enviado por medio del Usertask querido para marcar como completado
                 * contenga el SprintId, ya que si no tiene es porque no se ha iniciado el sprint, porque que al iniciarlo, se le
                 asigna un sprintId a las Usertasks seleccionadas*/
                if (dto.SprintId is null)
                {
                    response.CanMarkSprintTaskItemAsCompleted = false;
                    return response;
                }
                
                bool taskContainsSprint = await _tic.Tasks.TaskContainsSprintAsync(dto.TaskId, token);

                if (!taskContainsSprint)
                {
                    response.CanMarkSprintTaskItemAsCompleted = false;
                    return response;
                }

                bool canMarksprintTaskItemAsCompleted =
                    await _tic.Sprints.CanMarkTaskItemAsCompletedAsync(dto.SprintId, token);

                if (!canMarksprintTaskItemAsCompleted)
                {
                    /* Si el sprint status no es begun, no se puede marcar como completada un taskItem */
                    response.CanMarkSprintTaskItemAsCompleted = false;
                    return response;
                }
                

                /* Puede marcar como completo y se encuentra en un sprint con estado Begun */
                response.CanMarkSprintTaskItemAsCompleted = true;
                await _repoRead.MarkTaskItemAsCompleted(dto.TaskItemId, username);
                

            }
            else
            {
                /* Puede marcar como completado y NO se encuentra en un sprint ya que no es SCRUM */
                response.CanMarkSprintTaskItemAsCompleted = true;
                await _repoRead.MarkTaskItemAsCompleted(dto.TaskItemId, username);
            }

            if (!await _repoRead.IsAnyTaskItemNotCompleted(dto.TaskId))
            {
                _messageBusClient.Publish("task_completed", dto.TaskId);
                response.TaskIsCompleted = true;
            }


            return response;
        }
        public async Task<string> AskToGeminiAsync(AskToAssistantDto dto)
        => await _tic.Gemini.AskToGeminiAsync(dto);

        public async Task<string> DeleteTaskItemsAsync(string taskId)
        => await _repoWrite.DeleteTaskItemsAsync(taskId);

        public async Task<UpdateTaskItemResponse> UpdateTaskItemAsync(UpdateTaskItemDto dto, string token)
        {
            var response = new UpdateTaskItemResponse();

            bool taskExists = await _tic.Tasks.TaskExistsAsync(dto.TaskId, token);
            response.TaskExists = taskExists;

            if (!taskExists) return response;
            
            bool taskItemExists = await _repoRead.TaskItemExistsAsync(dto.TaskItemId);

            response.TaskItemExists = taskItemExists;
            if (!taskItemExists) return response;

            bool isTaskOwner = await _tic.Tasks.IsTaskOwnerAsync(dto.TaskId, token);
            response.IsTaskOwner = isTaskOwner;

            if (!isTaskOwner) return response;

            bool isAlreadyCompleted = await _repoRead.IsAlreadyCompletedAsync(dto.TaskItemId);
            response.IsAlreadyCompleted = isAlreadyCompleted;
            if (isAlreadyCompleted) return response;

            await _repoWrite.UpdateTaskItemAsync(dto);

            return response;

        }
    }
}
