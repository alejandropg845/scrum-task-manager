using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.Sprints.Interfaces;
using TaskManager.Sprints.Services;

namespace TaskManager.Tests
{
    public class Sprints_Tests
    {
        private readonly SprintsService _sprintsService;
        private readonly Mock<ISprintWriteRepository> _sprintsWriteRepo;
        private readonly Mock<ISprintReadRepository> _sprintsReadRepo;

        private readonly Mock<IMongoClient> _mongoClient;
        private readonly Mock<IMessageBusClient> _messageBus;
        private readonly Mock<ILogger<SprintsService>> _logger;
        private readonly Mock<ITasksClient> _tasksClient;
        private readonly Mock<IGroupsRolesClient> _groupRolesClient;
        public Sprints_Tests()
        {
            _sprintsWriteRepo = new Mock<ISprintWriteRepository>();
            _sprintsReadRepo = new Mock<ISprintReadRepository>();
            _mongoClient = new Mock<IMongoClient>();
            _messageBus = new Mock<IMessageBusClient>();
            _logger = new Mock<ILogger<SprintsService>>();
            _tasksClient = new Mock<ITasksClient>();
            _groupRolesClient = new Mock<IGroupsRolesClient>();

            _sprintsService = new SprintsService(
                _sprintsWriteRepo.Object,
                _mongoClient.Object,
                _messageBus.Object,
                _logger.Object,
                _tasksClient.Object,
                _groupRolesClient.Object,
                _sprintsReadRepo.Object
            );
        }
        [Fact]
        public async Task CreateSprintAsync_ShouldReturnCreatedSprint()
        {
            //Arrange
            int sprintNumber = 0;

            _sprintsWriteRepo.Setup(r => r.CreateSprintAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()
            )).Callback<string, string, int>((_, _, sprintN) => sprintNumber = sprintN)
            .ReturnsAsync(new ToSprintDto
            {
                SprintNumber = 2,
                ExpirationTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                SprintName = "sprintName123",
                Status = "created"
            });

            //Act 
            var createdSprint = await _sprintsService.CreateSprintAsync("sprintId", "groupName", 2);

            //Asserts
            createdSprint.ExpirationTime.Should().NotBeNull();
            createdSprint.SprintNumber.Should().BeGreaterThanOrEqualTo(sprintNumber);
            createdSprint.Status.Should().BeEquivalentTo("created");

        }
    }
}
