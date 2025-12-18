
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaskManager.Common;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Services;
using TaskManager.Groups.Interfaces;
using TaskManager.Groups.Payloads;
using TaskManager.Groups.Responses;

namespace TaskManager.Groups.Services
{
    public class GroupsService : IGroupWriteService, IGroupReadService
    {
        private readonly ISprintsClient _sprintsClient;
        private readonly IGroupsRolesClient _groupsRolesClient;
        private readonly IUsersClient _usersClient;
        private readonly IGroupWriteRepository _writeRepo;
        private readonly IGroupReadRepository _readRepo;
        private readonly IMessageBusClient _messageBus;
        private readonly ILogger<IGroupWriteService> _logger;

        public GroupsService(ISprintsClient sprintsClient, IGroupsRolesClient groupsRolesClient, IUsersClient usersClient, IGroupWriteRepository groupRepository, IMessageBusClient messageBus, ILogger<IGroupWriteService> logger, IGroupReadRepository readRepo)
        {
            _sprintsClient = sprintsClient;
            _groupsRolesClient = groupsRolesClient;
            _usersClient = usersClient;
            _writeRepo = groupRepository;
            _messageBus = messageBus;
            _logger = logger;
            _readRepo = readRepo;
        }

        public async Task<(bool GroupExists, string? GroupName, string? GroupRoleName)> JoinGroupAsync(string username, string groupname, string token)
        {

            string groupName = groupname.RemoveDiacritics().Trim();

            //Comrpobar que el groupExists

            var groupExists = await _readRepo.GroupExistsAsync(groupName);

            if (groupExists)
            {
                // Se obtiene el role del usuario en el grupo que se unió
                var getUserGroupRole_task = _groupsRolesClient
                    .GetUserGroupRoleAsync(
                        groupName, 
                        token, 
                        null
                    );

                // Set group al usuario
                var setGroupToUser_task = _usersClient.SetGroupToUserAsync(groupName, token, null);

                await Task.WhenAll(getUserGroupRole_task, setGroupToUser_task);

                string? userGroupRole = await getUserGroupRole_task;


                return new(groupExists, groupName, userGroupRole);
            }

            return new(groupExists, null, null);

        }

        public async Task<CreateGroupResponse> CreateGroupAsync(string username, string groupname, bool isScrum, string token)
        {
            string groupName = groupname.RemoveDiacritics().Trim();

            var response = new CreateGroupResponse();

            bool groupNameAlreadyTaken = true;
            string randomizedGroupName = string.Empty;

            while (groupNameAlreadyTaken)
            {
                randomizedGroupName = Extensions.RandomizeGroupName(groupName);
                groupNameAlreadyTaken = await _readRepo.GroupExistsAsync(randomizedGroupName);
            }

            /* Agregar el userGroupRole por default, siendo este Product Owner */
            if (isScrum)
            {
                
                var body = new SetUserGroupRoleDto(randomizedGroupName, "product owner");

                string sprintId = Guid.NewGuid().ToString();

                try
                {
                    // Crear el role por default Product Owner
                    var task1 = _groupsRolesClient.SetUserGroupRoleAsync(
                        randomizedGroupName,
                        username,
                        token,
                        body
                    );

                    // Crear sprint
                    var task2 = _sprintsClient.CreateSprintAsync(sprintId, randomizedGroupName, token, body);

                    // Agregar el nombre del grupo a crear al usuario
                    var task3 = _usersClient.SetGroupToUserAsync(randomizedGroupName, token, null);

                    // Crear grupo
                    var task4 = _writeRepo.AddGroupAsync(randomizedGroupName, username, isScrum);

                    // Agregar sprintId al grupo creado
                    var task5 = _writeRepo.SetSprintIdToGroupAsync(randomizedGroupName, sprintId);

                    await Task.WhenAll(task1, task2, task3, task4, task5);
                    

                } catch
                {

                    //  Al ser nuestro único role el creado (se crea el grupo), pasamos el nombre del group.
                    _messageBus.Publish("delete_group_roles", randomizedGroupName);

                    //  Se crea el sprint inicial (1), por lo que lo eliminamos reusando el método para
                    // eliminar todos los sprints de un group.
                    _messageBus.Publish("remove_sprints", randomizedGroupName);

                    // Somos el único member del group, reutilizamos método.
                    _messageBus.Publish("delete_members_group", randomizedGroupName);

                    var payload = new DeleteGroup { GroupName = randomizedGroupName, Username = username };
                    _messageBus.Publish("delete_group", payload);


                    throw;
                }

            }
            else
            {
                try
                {
                    // Agregar group en el documento User
                    var task1 = _usersClient.SetGroupToUserAsync(randomizedGroupName, token, null);

                    // Crear group
                    var task2 = _writeRepo.AddGroupAsync(randomizedGroupName, username, isScrum);

                    await Task.WhenAll(task1, task2);
                } catch
                {
                    var payload = new DeleteGroup { GroupName = groupName, Username = username };

                    _messageBus.Publish("delete_user_group", username);
                    _messageBus.Publish("delete_group", payload);

                    throw;
                }



            }

            response.GroupName = randomizedGroupName;
            return response;
        }
        public async Task<RemoveGroupResponse> RemoveGroupAsync(string username, string groupName)
        {
            var response = new RemoveGroupResponse();

            bool groupExists = await _readRepo.GroupExistsAsync(groupName);

            response.GroupExists = groupExists;
            if (!groupExists) return response;


            bool isScrum = await _readRepo.IsScrumAsync(groupName);

            try
            {

                var payload = new DeleteGroup { GroupName = groupName, Username = username };

                _messageBus.Publish("delete_group", payload);

                _messageBus.Publish("delete_members_group", groupName);

                if (isScrum)
                {
                    _messageBus.Publish("delete_group_roles", groupName);
                    _messageBus.Publish("remove_sprints", groupName);
                }

                response.DeletedGroup = groupName;
                response.DeletedGroupOwnerName = username;

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to process request at {methodName}", nameof(RemoveGroupAsync));

                throw;
            }


        }

        public async Task<bool> IsScrumAsync(string groupName)
        => await _readRepo.IsScrumAsync(groupName);

        public async Task<bool> SetAddingTasksAllowed(string username, string groupName, bool isAllowed)
        => await _writeRepo.SetAddingTasksAllowed(username, groupName, isAllowed);
        public async Task<bool> IsAddingTasksAllowedAsync(string groupName)
        => await _readRepo.IsAddingTasksAllowedAsync(groupName);

        public async Task<(bool IsScrum, bool IsAddingTasksAllowed, bool IsGroupOwner)> GetInitialGroupInfoAsync(string groupName, string username)
        => await _readRepo.GetInitialGroupInfoAsync(groupName, username);

        public async Task<bool> GroupExistsAsync(string groupName)
        => await _readRepo.GroupExistsAsync(groupName);

        public async Task<bool> IsGroupOwnerAsync(string username, string groupName)
        => await _readRepo.IsGroupOwnerAsync(username, groupName);

        public async Task<JsonDocument?> SetSprintIdToGroupAsync(string groupName, string sprintId)
        => await _writeRepo.SetSprintIdToGroupAsync(groupName, sprintId);

        public async Task DeleteCreatedGroupAsync(string groupName)
        => await _writeRepo.DeleteCreatedGroupAsync(groupName);
    }
}
