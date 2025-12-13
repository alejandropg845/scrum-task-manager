using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Common.Documents;
using TaskManager.Common.Interfaces;
using TaskManager.Tasks.DTOs;
using TaskManager.Tasks.Interfaces;
using TaskManager.Tasks.Repositories;
using TaskManager.Tasks.Services;

namespace TaskManager.Tests
{
    public class Tasks_Tests
    {
        private readonly TasksService _tasksService;
        private readonly Mock<ITaskWriteRepository> _tasksWriteRepo;
        private readonly Mock<ITaskReadRepository> _tasksReadRepo;
        private readonly Mock<IMessageBusClient> _messageBusClient;
        private readonly Mock<ITaskClients> _clients;
        public Tasks_Tests()
        {
            _tasksWriteRepo = new Mock<ITaskWriteRepository>();
            _tasksReadRepo = new Mock<ITaskReadRepository>();
            _clients = new Mock<ITaskClients>();
            _messageBusClient = new Mock<IMessageBusClient>();

            _tasksService = new TasksService(
                _tasksWriteRepo.Object,
                _messageBusClient.Object,
                _tasksReadRepo.Object,
                _clients.Object
            );
        }

        [Fact]
        public async Task AddUserTaskAsync_ReturnsErrorMessageIf_IsAddingTasksAllowedFalse()
        {
            //Arrange
            _clients.Setup(c => c.Groups.IsAddingTasksAllowed(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);
            _clients.Setup(c => c.Groups.IsUserGroupOwnerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);


            //Act
            var dto = new CreateTaskDto { GroupName = "groupName123", Title = "Title123" };

            var result = await _tasksService.AddUserTaskAsync("alexwhitney", dto, "token");


            //Asserts
            result.ErrorMessage.Should().BeEquivalentTo("Task adding is restricted to group owner");
            result.ErrorMessage.Should().NotBeNullOrEmpty();
            _tasksWriteRepo.Verify(r => r.AddTaskAsync(new UserTask()), Times.Never);
            result.Should().Be((null, result.ErrorMessage));
        }
        [Fact]
        public async Task AddUserTaskAsync_WhenNotInGroup()
        {
            //Arrange
            UserTask? userTask = null;
            _tasksWriteRepo.Setup(r => r.AddTaskAsync(It.IsAny<UserTask>()))
                .Callback<UserTask>(t =>
                {
                    userTask = t;
                }).ReturnsAsync(() => userTask!);


            //Act
            var dto = new CreateTaskDto { GroupName = "null", Title = "Title123" };
            var result = await _tasksService.AddUserTaskAsync("alexwhitney", dto, "token");


            //Asserts
            result.ErrorMessage.Should().BeEmpty();
            result.userTask.Should().NotBeNull();
            result.userTask.Title.Should().BeEquivalentTo(dto.Title);
        }
        [Fact]
        public async Task DeleteTaskAsync_ShouldReturnTaskExistsFalse_IfTaskDoesntExist()
        {
            //Arrange
            _tasksReadRepo.Setup(r => r.TaskExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

            //Act
            var response = await _tasksService.DeleteTaskAsync("anyGroup", "anyTaskId", "anyToken", "anyUsername");

            //Asserts
            response.TaskExists.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteTaskAsync_ShouldAllowTheTaskOwnerOrProductOwnerDeleteTask()
        {
            //Arrange
            _tasksReadRepo.Setup(r => r.TaskExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
            _clients.Setup(r => r.Groups.IsScrumAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _tasksReadRepo.Setup(r => r.IsTaskOwnerAsync(
                It.IsAny<string>(),
                It.IsAny<string>())
            ).ReturnsAsync(true);

            _clients.Setup(c => c.GroupRoles.GetRoleNameAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>())
            ).ReturnsAsync("product owner");

            _tasksWriteRepo.Setup(r => r.DeleteTaskWithoutScrumAsync(It.IsAny<string>())).ReturnsAsync(new UserTask());

            //Act
            var response = await _tasksService.DeleteTaskAsync("anyGroup", "anyTaskId", "anyToken", "anyUsername");

            //Asserts
            response.DeletedTask.Should().NotBeNull();
            response.TaskCanBeDeleted.Should().BeTrue();
            _tasksReadRepo.Verify(r => r.IsTaskOwnerAsync("", ""), Times.Never);

        }

        [Fact]
        public async Task DeleteTaskAsync_ShouldSimplyDeleteTaskWhenNoGroup()
        {
            //Arrange
            _tasksWriteRepo.Setup(r => r.DeleteTaskWithoutScrumAsync(It.IsAny<string>())).ReturnsAsync(new UserTask());
            _tasksReadRepo.Setup(r => r.TaskExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
            //Act
            var response = await _tasksService.DeleteTaskAsync(null, "anyTaskId", "anyToken", "anyUsername");

            //Asserts
            response.DeletedTask.Should().NotBeNull();
            response.TaskCanBeDeleted.Should().BeTrue();
        }
        [Theory]
        [InlineData("null")]
        [InlineData(null)]
        [InlineData("")]
        public async Task DeleteTaskAsync_ShouldNotExecuteGroupMethods(string? groupName)
        {
            //Arrange
            _tasksReadRepo.Setup(r => r.TaskExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

            //Act
            var response = await _tasksService.DeleteTaskAsync(groupName, "", "", "");

            //Asserts
            _clients.Verify(r => r.Groups.IsScrumAsync("", ""), Times.Never);
            _tasksReadRepo.Verify(r => r.IsTaskOwnerAsync("", ""), Times.Never);
            _tasksWriteRepo.Verify(r => r.DeleteTaskWithoutScrumAsync(""), Times.Once);
            response.TaskCanBeDeleted.Should().BeTrue();
        }
        [Fact]
        public async Task MarkTaskAsCompletedAsync_MarkTaskStatusAsCompletedWhenAllTaskItemsHaveBeenMarkedAsCompleted()
        {
            //Arrange
            _tasksWriteRepo.Setup(r => r.SetTaskAsCompletedAsync(It.IsAny<string>())).ReturnsAsync(true);

            //Act
            var allOk = await _tasksService.SetTaskAsCompletedAsync("anyTaskId");

            //Asserts
            allOk.Should().BeTrue();
        }
    }
}
