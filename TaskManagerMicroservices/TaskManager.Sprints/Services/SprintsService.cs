using Microsoft.AspNetCore.Components.Web;
using Microsoft.Playwright;
using MongoDB.Driver;
using System.Threading.Tasks;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.Sprints.Interfaces;
using TaskManager.Sprints.Payloads;
using TaskManager.Sprints.Responses;

namespace TaskManager.Sprints.Services
{
    public class SprintsService : ISprintWriteService, ISprintReadService
    {
        private readonly ISprintWriteRepository _repoWrite;
        private readonly ISprintReadRepository _repoRead;
        private readonly IMongoClient _mongoClient;
        private readonly IMessageBusClient _messageBus;
        private readonly ILogger<SprintsService> _logger;
        private readonly ITasksClient _tasksClient;
        private readonly IGroupsRolesClient _groupsRolesClient;
        public SprintsService(ISprintWriteRepository repo, IMongoClient mongoClient, IMessageBusClient messageBus, ILogger<SprintsService> logger, ITasksClient t, IGroupsRolesClient groupsRolesClient, ISprintReadRepository repoRead)
        {
            _repoWrite = repo;
            _mongoClient = mongoClient;
            _messageBus = messageBus;
            _logger = logger;
            _tasksClient = t;
            _groupsRolesClient = groupsRolesClient;
            _repoRead = repoRead;
        }
        public async Task<List<ToSprintDto>> GetGroupSprintsAsync(string groupName)
        => await _repoRead.GetGroupSprintsAsync(groupName);

        public async Task<(ToSprintDto CurrentSprint, string? PreviousSprint)> GetPreviousAndCurrentSprintAsync(string groupName)
        {
            var currentSprint_task = _repoRead.GetCurrentSprintAsync(groupName);

            var previousSprintId_task = _repoRead.GetPreviousSprintIdAsync(groupName);

            await Task.WhenAll(currentSprint_task, previousSprintId_task);

            return new(await currentSprint_task, await previousSprintId_task);

        }
        public async Task<BeginSprintResponse> BeginSprintAsync(BeginSprintDto dto)
        {
            DateTimeOffset expirationTime = DateTimeOffset.UtcNow.AddDays(dto.WeeksNumber * 7);

            var sprintBeforeUpdate = await _repoWrite.BeginSprintAsync(dto, expirationTime);
            
            try 
            {

                var obj = new SprintInfoForTask(dto.TasksIds, sprintBeforeUpdate.Id);

                _messageBus.Publish("tasks_sprint", obj);

            }
            catch
            {
                var payload = new RevertSprintStatus
                {
                    GroupName = dto.GroupName,
                    SprintId = sprintBeforeUpdate.Id,
                    Status = "created"
                };

                _messageBus.Publish("revert_sprint_status", payload);
                _messageBus.Publish("revert_in_progress_tasks_status", sprintBeforeUpdate.Id);

                throw;
            }

            return new BeginSprintResponse
            {
                TasksIds = dto.TasksIds,
                ExpirationTime = expirationTime,
                SprintId = sprintBeforeUpdate.Id,
                SprintName = dto.SprintName,
                RemainingTime = DateTimeOffset.UtcNow - expirationTime
            };
        }

        public async Task<(string completedSprintId, string createdSprintId)> CycleSprintAsync(SprintToComplete s, string token)
        {
            using var transaction = await _mongoClient.StartSessionAsync();
            transaction.StartTransaction();
            try
            {
                // Marcar como completado el sprint
                await _repoWrite.SetSprintAsCompletedAsync(s.CompletedSprintId, transaction);

                // Iniciar un nuevo sprint
                var newSprint = new Sprint()
                {
                    GroupName = s.Groupname,
                    Id = s.NewSprintId,
                    SprintExpiration = null,
                    Status = "created",
                    SprintNumber = s.SprintNumber + 1,
                };

                await _repoWrite.AddSprintAsync(newSprint, transaction);

                await transaction.CommitTransactionAsync();

                return new(s.CompletedSprintId, newSprint.Id);

            }
            catch 
            {
                await transaction.AbortTransactionAsync();
               
                throw;
            }

        }

        public async Task RevertCycledSprintAsync(string groupName, string completedSprintId, string createdSprintId)
        {
            using var transaction = await _mongoClient.StartSessionAsync();
            transaction.StartTransaction();
            try
            {
                await _repoWrite.DeleteSprintAsync(createdSprintId, transaction);

                await _repoWrite.RevertSprintStatusAsync(completedSprintId, groupName, "begun", transaction);

                await transaction.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error para eliminar el sprint marcado como completado (saga)\n" +
                    "Excepción: {Msg}\n" +
                    "StackTrace: {StackTrace}",
                    ex.Message, ex.StackTrace
                );

                await transaction.AbortTransactionAsync();
                throw;
            }

        }

        public async Task<ToSprintDto> CreateSprintAsync(string sprintId, string groupName, int sprintNumber)
        => await _repoWrite.CreateSprintAsync(sprintId, groupName, sprintNumber);

        public async Task<bool> CanMarkSprintTaskItemAsCompletedAsync(string sprintId)
        => await _repoRead.CanMarkSprintTaskItemAsCompletedAsync(sprintId);

        public async Task<int> GetSprintNumberAsync(string groupName)
        => await _repoRead. GetSprintNumberAsync(groupName);

        public async Task DeleteSprintAsync(string sprintId)
        => await _repoWrite.DeleteSprintAsync(sprintId, null);

        public async Task<(byte[] PdfBytes, bool IsAuthorized)> GenerateSummaryAsync(string token, string groupName)
        {

            bool isAuthorized = await _groupsRolesClient.IsAuthorizedByGroupRoleAsync(groupName, token);

            if (!isAuthorized) return new([0], isAuthorized);

            List<ToSprintDto> sprints = await _repoRead.GetCompletedSprintsForSummaryAsync(groupName);

            if (sprints.Count == 0) return new([], isAuthorized);

            var tasks = await _tasksClient.GetSprintsTasksAsync(token, groupName);

            foreach (var sprint in sprints)

                sprint.Tasks = tasks.Where(t => t.SprintId == sprint.Id).ToList();

            string html = SetHTMLContent(sprints);

            string filePath = Path.Combine(AppContext.BaseDirectory, "summary_styles.txt");

            string css = File.ReadAllText(filePath);

            css += html;

            css += @"</body></html>";

            byte[] bytes = await GeneratePdfAsync(css);

            return new(bytes, isAuthorized);
        }

        private static string SetHTMLContent(List<ToSprintDto> sprints)
        {
            string close = @"</div>";

            string sprintsContainer = @" <div class=""sprints-container"">";

            foreach (var sprint in sprints)
            {
                if (sprint.SprintNumber == 2)
                {
                    Console.WriteLine();
                }

                bool sprintNotCompleted = sprint.Tasks.Any(t => t.Status != "completed");

                string sprintName = $@"

                        <p class=""sprint-name"">
                            Sprint #{sprint.SprintNumber}: {sprint.SprintName}<span class=""{(sprintNotCompleted ? "status-not-completed" : "status-completed")}"">{(sprintNotCompleted ? "Not completed" : "Completed")}</span>
                        </p>

                ";

                sprintsContainer += sprintName;

                string tasksContainer = @"<div class=""tasks-container"">";

                foreach (var task in sprint.Tasks)
                {
                    string taskContainer = @"<div class=""task-container"">";

                    string taskTitle = $@"
                            <p class=""task-title"">{task.Title}</p>
                    ";

                    taskContainer += taskTitle;

                    string taskItemsContainer = @"<div class=""task-items-container"">";

                    foreach (var taskItem in task.TaskItems)
                    {
                        string status = !taskItem.IsCompleted ? "task-item-not-completed" : "";

                        string taskItemm = $@"
                        
                            <div class=""task-item-content"">
                                <p class=""task-item-title {status}"">{taskItem.Content}</p>
                                <p class=""task-item-responsable"">{taskItem.AssignToUsername}</p>
                            </div>                            

                        ";

                        taskItemsContainer += taskItemm;

                    }

                    taskItemsContainer += close;

                    taskContainer += taskItemsContainer;

                    string taskStatus = $@"

                        <div class=""completed-status-container"">
                            <p class=""task-status {(task.Status == "completed" ? "status-completed" : "status-not-completed")}"">{(task.Status == "completed" ? "Completed" : "Not completed")}</p>
                        </div>                                     

                    ";

                    taskContainer += taskStatus;

                    taskContainer += close;

                    tasksContainer += taskContainer;
                }

                /* Se acaban las tasks */

                tasksContainer += close;

                sprintsContainer += tasksContainer;

            }

            /* Se acaban los sprints */

            sprintsContainer += close;

            return sprintsContainer;
        }


        private static async Task<byte[]> GeneratePdfAsync(string html)
        {
            using var playwright = await Playwright.CreateAsync();

           
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var page = await browser.NewPageAsync();

           
            await page.SetContentAsync(html);

           
            byte[] pdfBytes = await page.PdfAsync(new()
            {
                Format = "A4",
                PrintBackground = true,
                Margin = new Margin
                {
                    Top = "20px",
                    Right = "20px",
                    Bottom = "20px",
                    Left = "20px"
                },
                
            });

            await browser.CloseAsync();

            return pdfBytes;
        }

    }
}
