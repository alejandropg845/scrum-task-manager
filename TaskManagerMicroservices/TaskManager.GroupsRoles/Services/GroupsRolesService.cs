using MongoDB.Driver;
using System.Threading.Tasks;
using TaskManager.Common.DTOs;
using TaskManager.GroupsRoles.Interfaces;
using TaskManager.GroupsRoles.Responses;

namespace TaskManager.GroupsRoles.Services
{
    public class GroupsRolesService : IGroupsRolesService
    {
        private readonly IGroupRolesWriteRepository _repoWrite;
        private readonly IGroupRolesReadRepository _repoRead;
        private readonly IMongoClient _mongoClient;
        private readonly ILogger<GroupsRolesService> _logger;

        public GroupsRolesService(IGroupRolesWriteRepository repo, IMongoClient mongoClient, ILogger<GroupsRolesService> logger, IGroupRolesReadRepository repoRead)
        {
            _repoWrite = repo;
            _repoRead = repoRead;
            _mongoClient = mongoClient;
            _logger = logger;
        }
        public async Task<SetGroupRoleResponse> SetGroupRoleAsync(SetUserGroupRoleDto dto, string username, string currentUser)
        {
            _logger.LogInformation("Inicio para agregar groupRole en el " +
                        "group {groupName} y para el usuario {username}"
                        , dto.GroupName, username);

            var response = new SetGroupRoleResponse();

           
            bool productOwnerExists = await _repoRead.ProductOwnerExistsAsync(dto.GroupName);

            /* Product Owner ya existe quiere decir que el grupo ya es existente y no está creando uno nuevo*/
            if (productOwnerExists)
            {

                var isProductOwner = await _repoRead.UserIsProductOwnerAsync(dto.GroupName, currentUser);

                response.IsProductOwner = isProductOwner;
                if (!isProductOwner) return response;

                using var transaction = await _mongoClient.StartSessionAsync();
                transaction.StartTransaction();

                try
                {
                    

                    if (dto.RoleName == "scrum master")
                    {
                        /* Obtener scrum master si existe, o sino null */
                        var scrumMasterRoleUser_T = _repoRead.GetScrumMasterAsync(dto.GroupName);

                        /* Obtener el groupRole del usuario al que se le asigna el rol Scrum Master */
                        var getUserGroupRole_T = GetGroupRoleNameAsync(dto.GroupName, username);

                        await Task.WhenAll(scrumMasterRoleUser_T, getUserGroupRole_T);

                        var scrumMasterRoleUser = await scrumMasterRoleUser_T;
                        string? userGroupRole = await getUserGroupRole_T;

                        Common.Documents.GroupsRoles? groupRole = null;

                        /* Scrum Master ya existia/ya habia sido asignado */
                        if (scrumMasterRoleUser is not null)
                        {
                            groupRole = await _repoWrite.RemoveScrumMasterAndSetToOtherAsync(
                                userGroupRole,
                                scrumMasterRoleUser.UserName,
                                username,
                                dto.GroupName,
                                dto.RoleName,
                                transaction
                            );

                            response.IsSwitchingScrumMaster = true;
                            response.UserThatWasScrumMaster = scrumMasterRoleUser.UserName;
                            response.UserThatIsScrumMaster = username;

                        }
                        else //ScrumMaster no existe
                        {
                            if (userGroupRole is not null)
                            {
                                // El usuario a cambiarle el role ya contaba con un role, entonces hacemos update
                                groupRole = await _repoWrite.UpdateGroupRoleAsync(username, dto.GroupName, dto.RoleName, null);
                            }
                            else
                            {
                                // No existe ningun role entonces creamos uno con scrum master
                                groupRole = await _repoWrite.CreateGroupRoleAsync(username, dto.RoleName, dto.GroupName, null);
                            }
                        }
                        response.GroupRole = groupRole;
                    }
                    else // No es Scrum Master
                    {
                        /* Verificar que el product owner está dando su propio rol */
                        string? userThatAssignedProductOwner = await _repoWrite.ProductOwnerAssignesOwnRoleAsync(currentUser, dto.GroupName, dto.RoleName, transaction);

                        if (userThatAssignedProductOwner is not null)

                            response.UserThatAssignedProductOwner = userThatAssignedProductOwner;

                        string? userToAssignRole = await GetGroupRoleNameAsync(dto.GroupName, username);

                        /* El usuario no contaba con un role asignado al grupo porque no se encontró un GroupRole del grupo*/
                        if (userToAssignRole is null)

                            /* Se crea un nuevo groupRole */
                            await _repoWrite.CreateGroupRoleAsync(username, dto.RoleName, dto.GroupName, transaction);

                        else // El usuario ya contaba con un role asignado al grupo, por lo que se modifica
                            await _repoWrite.UpdateGroupRoleAsync(username, dto.GroupName, dto.RoleName, transaction);

                        var role = new Common.Documents.GroupsRoles
                        {
                            GroupName = dto.GroupName,
                            RoleName = dto.RoleName,
                            UserName = username
                        };

                        response.GroupRole = role;

                    }
                    await transaction.CommitTransactionAsync();
                    response.IsProductOwner = isProductOwner;

                    return response;

                } catch (Exception ex)
                {
                    _logger.LogError("Error al agregar groupRole en el " +
                        "group {groupName} y para el usuario {username}\n" +
                        "Excepción: {Msg}\nStackTrace: {StackTrace}"
                        , dto.GroupName, username, ex.Message, ex.StackTrace);

                    await transaction.AbortTransactionAsync();
                    response.IsTransactionError = true;
                    return response;
                }
            }

            /* Product Owner no existe quiere decir que el grupo es nuevo, por lo que 
             damos por default el GroupRole de Product Owner al creador*/
            var newGroupRole = await _repoWrite.CreateGroupRoleAsync(currentUser, dto.RoleName, dto.GroupName, null);

            response.GroupRole = newGroupRole;

            _logger.LogInformation("Final correcto para agregar groupRole en el " +
            "group {groupName} y para el usuario {username}"
            , dto.GroupName, username);

            return response;

        }
        public async Task<string?> GetGroupRoleNameAsync(string groupName, string username)
        => await _repoRead.GetGroupRoleNameAsync(groupName, username);

        public async Task<List<GroupRoleDto>> GetUsersGroupRolesAsync(string groupName)
        => await _repoRead.GetUsersGroupRolesAsync(groupName);

        public async Task<string> RemoveGroupsRolesAsync(string groupName)
        => await _repoWrite.RemoveGroupsRolesAsync(groupName);
    }
}
