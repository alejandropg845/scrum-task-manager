using Google.Apis.Auth;
using MongoDB.Driver;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Users.Interfaces.Repository;

namespace TaskManager.Users.Repositories
{
    public class UsersRepository : IUserManagementRepository, IUserAuthRepository, IUserInfoRepository
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly FilterDefinitionBuilder<User> _filter = Builders<User>.Filter;
        public UsersRepository(IMongoCollection<User> collection)
        {
            _usersCollection = collection;

        }
        public async Task<IReadOnlyList<ToUserDto>> GetOnlyUsersAsync(string groupName)
        {
            var filter = _filter.Eq(u => u.GroupName, groupName);

            var projection = Builders<User>.Projection.Expression(u => new ToUserDto
            {
                Username = u.Username,
                GroupName = u.GroupName!,
                GroupRole = u.GroupRole // <== GroupRole se rellena después
            });

            var users = await _usersCollection.Find(filter).Project(projection).ToListAsync();

            return users;
        }
        public string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

        public bool VerifyPassword(string password, string storedHashedPassword) 
        => BCrypt.Net.BCrypt.Verify(password, storedHashedPassword);
        public async Task<bool> EmailExistsAsync(string email)
        {
            var filter = _filter.Eq(u => u.Email, email);

            return await _usersCollection.Find(filter).AnyAsync();
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            var filter = _filter.Eq(u => u.Username, username);

            return await _usersCollection.Find(filter).AnyAsync();
        }
        public async Task<User?> GetUserAsync(string username)
        {
            var filter = _filter.Eq(u => u.Username, username);

            return await _usersCollection.Find(filter).FirstOrDefaultAsync();
        }
        public async Task AddUserAsync(User user)
        => await _usersCollection.InsertOneAsync(user);
        public async Task<bool> IsGoogleAccountAsync(string username)
        {
            var filter = _filter.And(
                _filter.Eq(u => u.Username, username),
                _filter.Eq(u => u.Email, "google account")
            );

            return await _usersCollection.Find(filter).AnyAsync();
        }
        
        public async Task<User?> GetGoogleAccount(string googleId)
        {
            var filter = _filter.Eq(u => u.Id, googleId);

            return await _usersCollection.Find(filter).FirstOrDefaultAsync();
        }
        public async Task<(string Name, string Id, bool IsError)> ValidateGoogleTokenAsync(string idToken, string clientId)
        {
            try
            {

                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { clientId }
                };
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                return new(payload.Name, payload.Subject, false);

            } catch 
            {
                return new("", "", true);
            }
        }
        public async Task<string?> GetUserGroupNameAsync(string username)
        {
            var projection = Builders<User>.Projection.Expression(u => u.GroupName);

            return await _usersCollection.Find(_filter.Eq(u => u.Username, username))
                .Project(projection).FirstOrDefaultAsync();

        }
        public async Task<string> SetGroupToUserAsync(string username, string? groupName)
        {
            var filter = _filter.Eq(u => u.Username, username.ToLower());

            var update = Builders<User>.Update.Set(u => u.GroupName, groupName);

            var updatedUser = await _usersCollection
                .FindOneAndUpdateAsync(filter, update);

            return username;
        }

        public async Task RemoveGroupFromUsersAsync(string groupName)
        {
            var filter = _filter.Eq(u => u.GroupName, groupName);

            var update = Builders<User>.Update
                .Set(u => u.GroupName, null)
                .Set(u => u.GroupRole, null);

            await _usersCollection.UpdateManyAsync(filter, update);

        }
        public async Task<(string? GroupName, string AvatarBgColor)> GetGroupNameAndAvatarBgColorAsync(string username)
        {
            var filter = _filter.Eq(u => u.Username, username);

            var projection = Builders<User>
                .Projection
                .Expression(u => new ValueTuple<string?, string>(u.GroupName, u.AvatarBgColor));

            /* Obtener el groupName y el avatarBgColor */
            (string? groupName, string avatarBgColor) x = await _usersCollection
                .Find(filter).Project(projection).SingleOrDefaultAsync();

            return new(x.groupName, x.avatarBgColor);
        }


        public async Task LeaveGroupAsync(string username, string groupName)
        {
            var filter = _filter.And(
                _filter.Eq(u => u.Username, username),
                _filter.Eq(u => u.GroupName, groupName)
            );

            var update = Builders<User>.Update.Set(u => u.GroupName, null);

            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task SetRecoveryCodeAndExpirationTimeAsync(string email, string guid)
        {
            var filter = _filter.Eq(u => u.Email, email);

            var update = Builders<User>.Update
                .Set(u => u.RecoveryCode, guid)
                .Set(u => u.RecoveryExpirationTime, DateTimeOffset.UtcNow.AddMinutes(30));

            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task ChangeUserPasswordAsync(string password, string userId)
        {
            var update = Builders<User>.Update
                    .Set(u => u.RecoveryCode, null)
                    .Set(u => u.RecoveryExpirationTime, null)
                    .Set(u => u.Password, BCrypt.Net.BCrypt.HashPassword(password));

            await _usersCollection.UpdateOneAsync(_filter.Eq(u => u.Id, userId), update);
        }
        public async Task<(string userId, DateTimeOffset? RecoveryExpirationTime)> CheckReceivedRecoveryCodeAsync(string recoveryCode, string email)
        {
            /* Filtro para identificar al usuario */
            var filter = _filter.And(
                _filter.Eq(u => u.RecoveryCode, recoveryCode),
                _filter.Eq(u => u.Email, email)
            );

            var projection = Builders<User>.Projection.Expression(u => new { u.Id, u.RecoveryExpirationTime });

            var result = await _usersCollection.Find(filter)
                .Project(projection).FirstOrDefaultAsync();

            return new(result.Id, result.RecoveryExpirationTime);
        
        }
        public async Task DeleteSetGroupAsync(string username)
        {
            var filter = _filter.Eq(u => u.Username, username);

            var update = Builders<User>.Update.Set(u => u.GroupName, null);

            await _usersCollection.UpdateOneAsync(filter, update);

        }
    }
}