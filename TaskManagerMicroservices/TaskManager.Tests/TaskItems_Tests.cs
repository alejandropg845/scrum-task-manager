using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.TaskItems.Interfaces;
using TaskManager.TaskItems.Services;

namespace TaskManager.Tests
{
    public class TaskItems_Tests
    {
        private readonly TaskItemsService _taskItemsService;
        private readonly Mock<ITaskItemsWriteRepository> _taskItemsWriteRepo;
        private readonly Mock<ITaskItemsReadRepository> _taskItemsReadRepo;
        private readonly Mock<ITaskItemClients> _clients;
        private readonly Mock<IMessageBusClient> _messageBus;
        public TaskItems_Tests()
        {
            _taskItemsWriteRepo = new Mock<ITaskItemsWriteRepository>();
            _taskItemsReadRepo = new Mock<ITaskItemsReadRepository>();
            _clients = new Mock<ITaskItemClients>();
            _messageBus = new Mock<IMessageBusClient>();


            _taskItemsService = new TaskItemsService(
                _taskItemsWriteRepo.Object,
                _messageBus.Object,
                _taskItemsReadRepo.Object,
                _clients.Object
            );
        }

        [Fact]
        public async Task CreateTaskItemAsync_ShouldReturnErrorIfTaskBelongsToSprint()
        {
            //Arrange
            _clients.Setup(r => r.Groups.IsScrumAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _clients.Setup(r => r.Tasks.TaskContainsSprintAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            //Act
            var dto = new CreateTaskItemDto(
                "anyTaskId",
                "anyContent",
                "assign",
                "groupName",
                "taskTitle1",
                "sprintId"
            );

            var response = await _taskItemsService.CreateTaskItemAsync(dto, "alexwhitney", "token");

            //Assert
            response.AssignToUserError.Should().BeFalse();
            response.ContainsScrum.Should().BeTrue();
            response.ti.Should().BeEquivalentTo(new TaskItem());

            _taskItemsWriteRepo.Verify(r => r.AddTaskItemAsync(dto), Times.Never);
        }
        [Fact]
        public async Task CreateTaskItemAsync_CreatesTaskItemWhenNoScrumAndSprint()
        {
            var taskItem = new TaskItem();
            //Arrange
            _taskItemsWriteRepo.Setup(r => r.AddTaskItemAsync(It.IsAny<CreateTaskItemDto>())).ReturnsAsync(taskItem);

            //Act
            var dto = new CreateTaskItemDto("", "", "", null, "", "");
            var result = await _taskItemsService.CreateTaskItemAsync(dto, "username123", "token123");

            //Asserts
            result.Should().BeEquivalentTo((taskItem, false, false));
            _clients.Verify(r => r.Groups.IsScrumAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _clients.Verify(r => r.Tasks.TaskContainsSprintAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _taskItemsWriteRepo.Verify(r => r.AddTaskItemAsync(It.IsAny<CreateTaskItemDto>()), Times.Once);

        }

        [Fact]
        public async Task DeleteTaskItemAsync_ReturnsTaskContainsSprint_IfContainsSprint()
        {
            //Arrange
            _clients.Setup(c => c.Groups.IsScrumAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _clients.Setup(c => c.Tasks.TaskContainsSprintAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            //Act
            var dto = new DeleteTaskItemDto("taskItemId", "anyGroupName", "taskId");
            await _taskItemsService.DeleteTaskItemAsync(dto, "token");

            //Assert
            _taskItemsWriteRepo.Verify(r => r.DeleteTaskItemAsync(""), Times.Never);

        }


        [Fact]
        public async Task DeleteTaskItemAsync_DeleteTaskItemWhenAllOk()
        {
            //Arrange
            _clients.Setup(c => c.Groups.IsScrumAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _clients.Setup(c => c.Tasks.TaskContainsSprintAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            _taskItemsWriteRepo.Setup(r => r.DeleteTaskItemAsync(It.IsAny<string>()));

            //Act
            var dto = new DeleteTaskItemDto("taskItemId", "anyGroupName", "taskId");
            bool result = await _taskItemsService.DeleteTaskItemAsync(dto, "token");

            //Assert
            result.Should().BeTrue();
            _taskItemsWriteRepo.Verify(r => r.DeleteTaskItemAsync(It.IsAny<string>()), Times.Once);


        }
        [Fact]
        public async Task SetTaskItemAsCompletedAsync_MarkAsCompletedOnlyWhenDoesntHaveSprintId()
        {
            //Arrange
            _taskItemsReadRepo.Setup(r => r.TaskItemExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            _clients.Setup(r => r.Tasks.TaskExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);


            _clients.Setup(c => c.Groups.IsScrumAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _clients.Setup(r => r.Sprints.CanMarkTaskItemAsCompletedAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _taskItemsReadRepo.Setup(r => r.IsAnyTaskItemNotCompleted(It.IsAny<string>())).ReturnsAsync(false);

            _clients.Setup(c => c.Tasks.TaskContainsSprintAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            //Act
            var dto = new MarkTaskItemAsCompletedDto("taskItemId", "tasId", "groupName", "sprintId");
            var response = await _taskItemsService.SetTaskItemAsCompletedAsync("username123", dto, "token");

            //Assert
            response.CanMarkSprintTaskItemAsCompleted.Should().BeTrue();
            _taskItemsReadRepo.Verify(r => r.MarkTaskItemAsCompleted(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _messageBus.Verify(c => c.Publish(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
