using MongoDB.Driver;
using Moq;
using System.Text.Json;
using TaskManager.Common;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Payloads;
using TaskManager.Users.Interfaces;
using TaskManager.Users.Interfaces.Clients;
using TaskManager.Users.Interfaces.Repository;
using TaskManager.Users.Interfaces.Service;
using TaskManager.Users.Responses;

namespace TaskManager.Users.Services
{
    public class UsersService : IUserAuthService, IUserManagementService, IUserInfoService
    {
        private readonly IMailRecoveryPasswordService _recoverPasswordService;
        private readonly IUserManagementRepository _repoManage;
        private readonly IUserAuthRepository _repoAuth;
        private readonly IUserInfoRepository _repoInfo;
        private readonly ITokenService _tokenService;
        private readonly IUserClients _uc;
        private readonly IMessageBusClient _messageBusClient;

        private readonly string _apiGatewayIssuer;
        private readonly string _apiGatewayAudience;
        private readonly string _clientId;
        public UsersService(IMailRecoveryPasswordService rps, 
            IConfiguration config, 
            IUserManagementRepository repo, 
            IUserAuthRepository repoAuth, 
            IUserInfoRepository repoProfile, 
            IMessageBusClient mbc,
            IUserClients uc,
            ITokenService tc
            )
        {
            _tokenService = tc;
            _uc = uc;
            _messageBusClient = mbc;
            _recoverPasswordService = rps;
            
            _repoManage = repo;
            _repoAuth = repoAuth;
            _repoInfo = repoProfile;
            _apiGatewayAudience = config["ApiGateway:Audience"]!;
            _apiGatewayIssuer = config["ApiGateway:Issuer"]!;
            _clientId = config["Credentials:ClientId"]!;
        }
        public async Task<IReadOnlyList<ToUserDto>> GetUsersAsync(string groupName, string token)
        {

            bool isScrum = await _uc.Groups.IsScrumAsync(groupName, token);

            IReadOnlyList<ToUserDto> users = [];

            Dictionary<string, string?>? roles_dictionary = null;

            if (isScrum)
            {
                var getUsers_T = _repoManage.GetOnlyUsersAsync(groupName);
                var getRoles_T = _uc.GroupsRoles.GetUsersGroupRolesAsync(groupName, token);

                await Task.WhenAll(getUsers_T, getRoles_T);

                var roles = await getRoles_T;
                var userss = await getUsers_T;

                var jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var groupRoles = (await getRoles_T)!
                    .Deserialize<List<GroupRoleDto>>(jsonSerializerOptions)!;

                /* Darle valor al diccionario */
                roles_dictionary = (groupRoles).ToDictionary(r => r.Username, r => r.RoleName);

                users = await getUsers_T;

                /* Sino es scrum, simplemente obtener el usuario sin GroupRole, siendo este null en cambio */
            }
            else users = await _repoManage.GetOnlyUsersAsync(groupName);

            /* Si el diccionario no es nulo, quiere decir que se obtuvieron datos, por lo que entró en Scrum */
            if (roles_dictionary is not null)
            {
                foreach (var user in users)
                    if (roles_dictionary.TryGetValue(user.Username, out string? role))
                        user.GroupRole = role;
            }
            else
            {
                /* El diccionario es nulo (no entró en SCRUM), por lo que el valor para cada groupRole de usuario es null */
                foreach (var user in users)
                    user.GroupRole = null;
            }

            return users;

        }


        public async Task<LoginUserResponse> LoginUserAsync(LoginUserDto dto)
        {
            string normalizedUsername = dto.Username.RemoveDiacritics().ToLower();

            var response = new LoginUserResponse();

            var user = await _repoInfo.GetUserAsync(dto.Username);

            response.UserDoesntExist = user is null;
            if (user is null) return response;

            bool isGoogleAccount = await _repoAuth.IsGoogleAccountAsync(dto.Username);
            if (isGoogleAccount) return response; /* <== Al retornar response si IsGoogleAccount, recae en IsCorrect = false
                                                         ya que el valor de IsCorrect no ha cambiado */
            bool IsCorrect = _repoAuth.VerifyPassword(dto.Password, user.Password);

            response.IsCorrect = IsCorrect;
            if (!IsCorrect) return response;

            /* Se requiere un JWT para pasar el apiGateway */
            string jwt = _tokenService.GenerateToken(normalizedUsername, _apiGatewayAudience, _apiGatewayIssuer);

            Token rToken = ExtendedConfigs.GenerateRefreshToken(user.Username);

            await _uc.Tokens.SaveRefreshTokenAsync(jwt, rToken);

            response.AccessToken = jwt;
            response.RefreshToken = rToken.RefreshToken;
            return response;

        }

        public async Task<RegisterUserResponse> RegisterUserAsync(RegisterUserDto dto)
        {
            string normalizedUsername = dto.Username.RemoveDiacritics().ToLower();

            var response = new RegisterUserResponse();

            bool userExists = await _repoAuth.UserExistsAsync(normalizedUsername);

            response.UserExists = userExists;
            if (userExists) return response;

            if (dto.Email is not null)
            {
                var emailExists = await _repoAuth.EmailExistsAsync(dto.Email);

                response.EmailExists = emailExists;
                if (emailExists) return response;
            }

            string hashedPassword = _repoAuth.HashPassword(dto.Password);

            var newUser = new User
            {
                Username = normalizedUsername,
                Password = hashedPassword,
                Id = Guid.NewGuid().ToString(),
                Email = dto.Email,
                AvatarBgColor = SetAvatarColor()
            };

            await _repoAuth.AddUserAsync(newUser);

            /* Se requiere un JWT para pasar el apiGateway */
            string jwt = _tokenService.GenerateToken(normalizedUsername, _apiGatewayAudience, _apiGatewayIssuer);

            Token rToken = ExtendedConfigs.GenerateRefreshToken(newUser.Username);

            await _uc.Tokens.SaveRefreshTokenAsync(jwt, rToken);

            response.AccessToken = jwt;
            response.RefreshToken = rToken.RefreshToken;
            return response;

        }

        public async Task<ContinueWithGoogleResponse> ContinueWithGoogleAsync(string tokenId)
        {
            var response = new ContinueWithGoogleResponse();

            var (Username, Id, IsError) = await _repoAuth.ValidateGoogleTokenAsync(tokenId, _clientId);

            if (IsError)
            {
                response.IsGoogleAuthError = IsError;
                return response;
            }

            /* En este caso, validamos que el id exista, ya que no hay mas usuarios con este id de Google */
            var user = await _repoAuth.GetGoogleAccount(Id);

            /* Si el user no existe, quiere decir que se está "registrando" */
            if (user is null)
            {
                /* Quitar todos los espacios vacios*/
                string formattedUsername = String.Concat(Username.Where(c => !Char.IsWhiteSpace(c)));

                /* Para cumplir la regla de que el username contenga 15 caracteres, lo que hacemos es 
                 * eliminar caracteres al final del Name, para despues agregar 
                 de 1 a 3 números como máximo*/

                /* el usuario tiene más de 12 caracteres */
                if (formattedUsername.Length > 12)
                    /* Eliminamos los caracteres desde la posición 12 (siendo 13, sin contar el 0 en el string) */
                    formattedUsername = formattedUsername.Remove(12);

                /* Eliminar caracteres especiales */
                string normalizedUsername = formattedUsername.RemoveDiacritics().ToLower();

                bool userNameExists = true;
                string newUsername = "";

                /* Creamos un username aleatorio para evitar crear un usuario con el mismo username que da Google*/
                while (userNameExists)
                {
                    newUsername = normalizedUsername + new Random().Next(1, 100);

                    /* Se ejecuta este ciclo hasta que no encuentre un usuario con el userName aleatorio */
                    userNameExists = await _repoAuth.UserExistsAsync(newUsername);
                }

                /* Creamos el nuevo usuario y lo guardamos en la BD */
                var newUser = new User
                {
                    Email = "google account",
                    GroupName = null,
                    GroupRole = null,
                    Id = Id,
                    Password = "",
                    RecoveryCode = null,
                    RecoveryExpirationTime = null,
                    Username = newUsername.ToLower(),
                    AvatarBgColor = SetAvatarColor()
                };

                await _repoAuth.AddUserAsync(newUser);

                /* Se requiere un JWT para pasar el apiGateway */
                string jsonWt = _tokenService.GenerateToken(newUser.Username, _apiGatewayAudience, _apiGatewayIssuer);

                Token refreshToken = ExtendedConfigs.GenerateRefreshToken(newUser.Username);

                await _uc.Tokens.SaveRefreshTokenAsync(jsonWt, refreshToken);

                response.AccessToken = jsonWt;
                response.RefreshToken = refreshToken.RefreshToken;
                return response;
            }

            /* Se requiere un JWT para pasar el apiGateway */
            string jwt = _tokenService.GenerateToken(user.Username, _apiGatewayAudience, _apiGatewayIssuer);

            Token rToken = ExtendedConfigs.GenerateRefreshToken(user.Username);

            await _uc.Tokens.SaveRefreshTokenAsync(jwt, rToken);

            response.AccessToken = jwt;
            response.RefreshToken = rToken.RefreshToken;
            return response;

        }
        public async Task<InitialInfoDto> GetUserInfoAsync(string username, string token)
        {

            // GET
            /* Obtener el groupName y el avatarBgColor */
            (string? groupName, string avatarBgColor) x = await _repoInfo.GetGroupNameAndAvatarBgColorAsync(username);

            string? groupName = x.groupName;
            string avatarBgColor = x.avatarBgColor;

            if (!string.IsNullOrEmpty(groupName) && groupName is not "") //El usuario se encuentra unido a un group
            {
                // GET
                var (isScrum, isAddingTasksAllowed, isGroupOwner) = await _uc.Groups.GetGroupFeatures(groupName, token);

                if (isScrum)
                {
                    // GET
                    var (userGroupRole, currentSprint, previousSprintId) =
                        await _uc.Groups.GetGroupRoleWithPreviousAndCurrentSprintAsync(groupName, token);

                    int sprintNumber = currentSprint.SprintNumber;
                    string status = currentSprint.Status;
                    DateTimeOffset? expirationTime = currentSprint.ExpirationTime;
                    string? sprintName = currentSprint.SprintName;
                    var remainingTime = DateTimeOffset.UtcNow - currentSprint.ExpirationTime;

                    string? finishedSprintName = null;
                    string? finishedSprintId = null;

                    /* Utilizamos esta variable sólo para verificar si entró o no a la expiración */
                    bool isExpired = false;


                    if (currentSprint.Status == "begun")

                        if (currentSprint.ExpirationTime <= DateTimeOffset.UtcNow)
                        {
                            /* - Marcar el sprint actual como completado y crear el siguiente sprint
                               - Marcar las tareas del sprint como completadas
                               - Agregar feedback a los usuarios del grupo con role Developer
                            */
                            await SetSprintAsCompletedAsync
                            (
                                currentSprint.Id,
                                groupName,
                                sprintNumber,
                                token
                            );

                            if (userGroupRole == "developer")
                            {
                                /* Enviamos para la info de finishedSprint la info de currentSprint */
                                isExpired = true;
                                finishedSprintName = $"Sprint #{currentSprint.SprintNumber}";
                                finishedSprintId = currentSprint.Id;
                            }

                            status = "created";
                            expirationTime = null;
                            sprintNumber++; // Sumamos el sprint en el primer retorno, ya que ya venció
                            sprintName = null;
                            remainingTime = null;
                        }

                    /* No entró a expiración */
                    if (!isExpired)
                    {
                        /* Verificar que previousSprintId no sea nulo (solo pasa si es el 1er sprint creado) */
                        if (previousSprintId is not null)
                        {
                            /* Obtener feedback con la info del sprint anterior */
                            var (SprintId, IsSubmited) = await _uc.Feedbacks.IsFeedbackSubmitedAsync(groupName, previousSprintId, token);

                            /* El usuario tenia el rol de Developer y se 
                             encontró un feedback */
                            if (SprintId is not null)
                            {
                                /* El feedback no está submited para el sprint anterior */
                                if (!IsSubmited)
                                {
                                    /* Enviamos para la info de finishedSprint la info de previousSprint */
                                    finishedSprintName = $"Sprint #{currentSprint.SprintNumber - 1}";
                                    finishedSprintId = previousSprintId;
                                }
                            }
                        }
                        /* No existe un sprint anterior, y tampoco el currentSprint está expirado, por lo que
                         * ninguno de los dos sprints está pendiente por feedback*/
                    }

                    return new InitialInfoDto
                    {
                        IsGroupOwner = isGroupOwner,
                        GroupName = groupName,
                        GroupRole = userGroupRole,
                        IsScrum = isScrum,
                        IsAllowed = isAddingTasksAllowed,
                        Status = status,
                        ExpirationTime = expirationTime,
                        SprintNumber = sprintNumber,
                        AvatarBgColor = avatarBgColor,
                        SprintName = sprintName,
                        RemainingTime = remainingTime,
                        FinishedSprintId = finishedSprintId,
                        FinishedSprintName = finishedSprintName
                    };

                }

                return new InitialInfoDto
                {
                    IsGroupOwner = isGroupOwner,
                    GroupName = groupName,
                    IsAllowed = isAddingTasksAllowed,
                    AvatarBgColor = avatarBgColor
                };

            }

            return new InitialInfoDto
            {
                AvatarBgColor = avatarBgColor
            };
        }
        private async Task SetSprintAsCompletedAsync(string sprintId, string groupName, int sprintNumber, string token)
        {
            string newSprintId = Guid.NewGuid().ToString();
            
            var body = new SprintToComplete(sprintId, newSprintId, groupName, sprintNumber);


            try
            {

                // Marcar el sprint como completado e iniciar un nuevo sprint (crear un sprint nuevo)
                var task1 = _uc.Sprints.CycleSprintAsync(token, body);

                // Marcar las tareas relacionadas al sprint como finished
                var task2 = _uc.Tasks.MarkSprintTasksAsFinishedAsync(token, sprintId);

                // Agregar feedbacks a users developers
                var task3 = _uc.Feedbacks.AddFeedbackToUsersAsync(groupName, sprintId, token);

                await Task.WhenAll(task1, task2, task3);



            } catch
            {

                // Revertir el sprint completed --> begun e inmediatamente eliminar el nuevo sprint que se crea después
                var revertCycledSprintPayload = new RevertCycledSprint
                {
                    GroupName = groupName,
                    CompletedSprintId = sprintId,
                    NewSprintId = newSprintId
                };

                _messageBusClient.Publish("revert_cycled_sprint", revertCycledSprintPayload);

                // Eliminar feedbacks a users del sprint y group 
                var payload = new { GroupName = groupName, SprintId = sprintId };
                _messageBusClient.Publish("delete_feedbacks", payload);

                // Revertir las tareas marcadas relacionadas al sprint como finished
                _messageBusClient.Publish("revert_tasks_finished_status", sprintId);

                throw;
            }

        }
        public async Task RecoverPasswordAsync(string email)
        {
            bool userExists = await _repoAuth.EmailExistsAsync(email);

            if (userExists)
            {
                string guid = Guid.NewGuid().ToString();

                await _repoAuth.SetRecoveryCodeAndExpirationTimeAsync(email, guid);

                await _recoverPasswordService.SendCodeToEmailAsync(email, guid);

            }

        }

        public async Task LeaveGroupAsync(string username, string groupName)
        => await _repoManage.LeaveGroupAsync(username, groupName);
        public async Task<ReceiveRecoveryCodeResponse> ReceiveRecoveryCodeAsync(string recoveryCode, string email, string password1, string password2)
        {

            var response = new ReceiveRecoveryCodeResponse();

            bool passwordsMatch = (password1 == password2);

            response.PasswordsMatch = passwordsMatch;

            if (!passwordsMatch) return response;

            var (userId, recoveryExpirationTime) = await _repoAuth.CheckReceivedRecoveryCodeAsync(recoveryCode, email);

            if (userId is not null && recoveryExpirationTime is not null)
            {
                bool isExpired = DateTimeOffset.UtcNow >= recoveryExpirationTime;

                response.IsExpired = isExpired;

                if (isExpired) return response;

                await _repoAuth.ChangeUserPasswordAsync(password1, userId);

                response.RecoveryCodeIsOk = true;
                return response;
            }

            response.RecoveryCodeIsOk = false;
            return response;
        }
        private static string SetAvatarColor()
        {

            string[] colors = [
            "#000000", "#20d109", "#181818",
            "#808080", "#ffd900", "#ff7070",
            "#00ffff", "#d2691e", "#dc143c",
            "#483d8b", "#008080", "#d400ffff"
            ];

            int randomizedIndex = new Random().Next(0, 11);

            return colors[randomizedIndex];
        }

        public async Task<string> SetGroupToUserAsync(string username, string? groupName)
        => await _repoManage.SetGroupToUserAsync(username, groupName);

        public async Task<string?> GetUserGroupNameAsync(string username)
        => await _repoInfo.GetUserGroupNameAsync(username);

    }
}
