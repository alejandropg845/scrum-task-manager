using Castle.Core.Logging;
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
using TaskManager.GroupsRoles.Interfaces;
using TaskManager.GroupsRoles.Responses;
using TaskManager.GroupsRoles.Services;

namespace TaskManager.Tests
{
    public class GroupRoles_Tests
    {
        private readonly Mock<IMongoClient> _mongoClient;
        private readonly GroupsRolesService _groupRolesService;
        private readonly Mock<IGroupRolesWriteRepository> _groupsWriteRepo;
        private readonly Mock<IGroupRolesReadRepository> _groupsReadRepo;
        private readonly Mock<IClientSessionHandle> _mockSession;
        private readonly Mock<ILogger<GroupsRolesService>> _logger;
        public GroupRoles_Tests()
        {
            _mongoClient = new Mock<IMongoClient>();
            _groupsWriteRepo = new Mock<IGroupRolesWriteRepository>();
            _groupsReadRepo = new Mock<IGroupRolesReadRepository>();
            _logger = new Mock<ILogger<GroupsRolesService>>();
            _mockSession = new Mock<IClientSessionHandle>();

            _mongoClient.Setup(c => c.StartSessionAsync(
                It.IsAny<ClientSessionOptions>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(() => _mockSession.Object);

            _groupRolesService = new GroupsRolesService(
                _groupsWriteRepo.Object,
                _mongoClient.Object,
                _logger.Object,
                _groupsReadRepo.Object
            );
        }

        [Fact]
        public async Task SetGroupRoleAsync_ShouldNotAllowChangeRolesIfUserIsNotProductOwner()
        {
            //Arrange
            _groupsReadRepo.Setup(r => r.ProductOwnerExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _groupsReadRepo.Setup(r => r.UserIsProductOwnerAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            //Act
            var dto = new SetUserGroupRoleDto("groupName", "anyRole");
            var response = await _groupRolesService.SetGroupRoleAsync(dto, "username123", "anyCurrentuser123");

            //Asserts
            response.IsProductOwner.Should().BeFalse();
        }

        [Fact]
        public async Task SetGroupRoleAsync_AllowUserToAssignAnyRole()
        {
            //Arrange


            _groupsReadRepo.Setup(r => r.ProductOwnerExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _groupsReadRepo.Setup(r => r.UserIsProductOwnerAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _groupsWriteRepo.Setup(r => r.ProductOwnerAssignesOwnRoleAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IClientSessionHandle>()))
                .ReturnsAsync((string?)null);

            _groupsReadRepo.Setup(r => r.GetGroupRoleNameAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string?)null);
            _groupsWriteRepo.Setup(r => r.CreateGroupRoleAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IClientSessionHandle>()))
                .ReturnsAsync(new Common.Documents.GroupsRoles());

            //Act
            var dto = new SetUserGroupRoleDto("groupName123", "developer");
            var response = await _groupRolesService.SetGroupRoleAsync(dto, "usernameThatReceivedGroupRole", "alexwhitney845");

            //Asserts
            response.IsProductOwner.Should().BeTrue();
            response.GroupRole.UserName.Should().BeEquivalentTo("usernameThatReceivedGroupRole");
            response.GroupRole.RoleName.Should().BeEquivalentTo(dto.RoleName);
            response.UserThatAssignedProductOwner = null;
        }
    }
}
