using MongoDB.Driver;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;
using TaskManager.Sprints.Interfaces;

namespace TaskManager.Sprints.Services
{
    public class RetrospectivesService : IRetrospectivesService
    {
        private readonly IRetrospectivesRepository _repo;
        private readonly IMongoClient _mongoClient;
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IGroupsRolesClient _groupsRolesClient;
        private readonly ILogger<RetrospectivesService> _logger;
        public RetrospectivesService(IRetrospectivesRepository repo, IMongoClient mongoClient, IFeedbackRepository feedbackRepository, IGroupsRolesClient groupsRolesClient, ILogger<RetrospectivesService> logger)
        {
            _repo = repo;
            _mongoClient = mongoClient;
            _feedbackRepository = feedbackRepository;
            _groupsRolesClient = groupsRolesClient;
            _logger = logger;
        }

        public async Task<List<ToRetrospectiveDto>> GetRetrospectivesAsync(string groupName)
        => await _repo.GetRetrospectivesAsync(groupName);
        public async Task<(ToRetrospectiveDto? retro, bool isError)> AddRetroAndFeedbackAsync(CreateRetrospectiveDto dto, string username)
        {
            _logger.LogInformation("Agregar retrospective y feedBack iniciado para {groupName} y por {username}", dto.GroupName, username);

            using var transaction = await _mongoClient.StartSessionAsync();
            transaction.StartTransaction();

            try
            {
                var addRetro_T = _repo.AddSprintRetroAsync(dto, transaction);
                var addFeedback_T = _feedbackRepository.MarkFeedbackAsSubmitedAsync(username, dto.GroupName, dto.SprintId, transaction);
                

                await Task.WhenAll(addRetro_T, addFeedback_T);

                await transaction.CommitTransactionAsync();
                var createdRetro = await addRetro_T;

                _logger.LogInformation("Agregar retrospective y feedBack finalizado para {groupName} y por {username}", dto.GroupName, username);
                return new(createdRetro, false);
            }
            catch (Exception ex) 
            {
                _logger.LogError(
                    "Error al agregar retrospective y feedback " +
                    "para {groupName} y por {username}\n" +
                    "Excepción: {Msg}\n" +
                    "StackTrace: {StackTrace}",
                    dto.GroupName, username, ex.Message, ex.StackTrace
                );

                await transaction.AbortTransactionAsync();
                return new(null, true);
            }
        }

        public async Task<bool> IsAuthorizedByGroupRoleAsync(string groupName, string token)
        => await _groupsRolesClient.IsAuthorizedByGroupRoleAsync(groupName, token);
        
    }
}
