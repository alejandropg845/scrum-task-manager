using MongoDB.Driver;
using TaskManager.Chats.Interfaces;
using TaskManager.Chats.Repositories;
using TaskManager.Common.Documents;
using TaskManager.Common.DTOs;

namespace TaskManager.Chats.Services
{
    public class DatesService : IDatesService
    {
        private readonly IDatesRepository _repo;
        private readonly IMessagesService _messagesService;
        private readonly IMongoClient _mongoClient;
        private readonly IMessagesRepository _messagesRepository;
        private readonly ILogger<DatesService> _logger;
        public DatesService(IDatesRepository repo, IMessagesService messagesService, IMongoClient mongoClient, IMessagesRepository messagesRepository, ILogger<DatesService> logger)
        {
            _repo = repo;
            _messagesService = messagesService;
            _mongoClient = mongoClient;
            _messagesRepository = messagesRepository;
            _logger = logger;
        }

        public async Task<GetMessagesResponse> GetMessagesAsync(GetMessagesDto dto)
        {
            var response = new GetMessagesResponse();

            response.Messages = [];

            /* Primera vez */
            if (dto.DatePage == 0 && dto.MessagesPage == 0)
            {
                /* Se obtiene por default el date más reciente junto con sus mensajes */
                var firstDate = await _repo.GetNextDateAsync(dto.GroupName, dto.DatePage);

                /* Retornamos noMessages como false ya que es la primera petición
                    * y no se están demandando mensajes */
                if (firstDate is null) return response;

                /* El usuario antes de realizar una paginación al actual Date, envía un mensaje, 
                 * debemos agregar al Skip de rows de la base de datos este mensaje
                  y lo hacemos sumandole el número de sentMessages al Skip, en este caso no es 
                relevante ya que es la primera obtención de mensajes y no hay mensajes enviados previamente.*/
                var messages = await _messagesService
                    .GetDateMessagesAsync(firstDate.Id, dto.MessagesPage, dto.SentMessages);

                firstDate.Messages = messages;

                response.MessagesDate = firstDate;
                response.DateId = firstDate.Id;
                return response;
            }

            /* El dateId enviado por parte del frontend va a ser el actual dateId guardado.
             * DateId será null cuando el frontend reciba noMoreMessages como true, por lo que 
             * hay que redigir la petición nuevamente al endpoint pero con dateId null para 
               traer ahora un nuevo date junto con sus mensajes*/


            if (dto.DateId is not null)
            {
                /* 1. La idea es obtener la fecha actual de UctNow para verificar si se están obteniendo
                 * mensajes de un date anterior.*/
                DateTime? dateFullTime = await _repo.GetDateFullDateTimeAsync(dto.DateId);

                /* 2. Luego, agregamos un ternario donde si sí es un Date anterior, lo mejor será no
                 * agregar estos sentMessages, porque como ya pasó el Date, los mensajes o el listado de 
                 mensajes que se van a obtener de este Date no resultarán afectados, es decir, siempre
                tendrán el mismo número de mensajes sin importar si se enviaron mensajes previamente.*/
                int sentMessages = (DateTimeOffset.UtcNow > dateFullTime) ? 0 : dto.SentMessages;

                var moreMessages = await _messagesService.GetDateMessagesAsync(dto.DateId, dto.MessagesPage, sentMessages);

                /* Retornamos no more messages */
                if (moreMessages.Count == 0)
                {
                    response.NoMoreMessages = true;
                    return response;
                }

                response.Messages = moreMessages;
                response.DateId = dto.DateId;
                return response;
            }

            /* DateId es null */

            /* También en este punto la paginación de MessagesPage volverá a ser 0 por parte del frontend */

            var nextDate = await _repo.GetNextDateAsync(dto.GroupName, dto.DatePage);

            if (nextDate is null)
            {
                response.NoMoreDates = true;
                return response;
            }

            /* Agregamos el valor de 0 por default, porque si el código se ejecutará hasta acá entonces es porque
             * vamos a obtener un nuevo Date, y como mencionamos anteriormente, los messages enviados 
             previamente no afectan en el número de messages de este Date a obtener, porque el número
            de mensajes no cambiará y siempre será el mismo. */

            var nextMessages = await _messagesService.GetDateMessagesAsync(nextDate.Id, dto.MessagesPage, 0);
            nextDate.Messages = nextMessages;

            response.MessagesDate = nextDate;
            response.DateId = nextDate.Id;
            return response;
        }
        public async Task<SendMessageResponse> SendMessageAsync(SendMessageDto dto, string username)
        {
            _logger.LogInformation("Inicio de envío de Message para {groupName} por usuario {username}", dto.GroupName, username);
            var response = new SendMessageResponse();
            var updatedDate = DateTimeOffset.UtcNow;

            var currentMessagesDate = await _repo.GetMessagesDateAsync(dto.GroupName);


            /* Primer mensaje enviado en el chat */
            if (currentMessagesDate is null)
            {
                var createdDate = await CreateDateWithMessageAsync(
                    updatedDate,
                    dto.Message,
                    dto.GroupName,
                    username,
                    dto.AvatarBgColor
                );

                if (createdDate is null)
                {
                    _logger.LogError("Error al enviar primer mensaje para groupName " +
                        "{groupName} por el usuario {username}", dto.GroupName, username);

                    response.IsTransactionError = true;
                    return response;
                }

                response.MessagesDate = createdDate;
                return response;
            }


            /* Si entra ya nos encontramos en un dia mayor al anterior */
            if (currentMessagesDate.MessagesFullDateInfo.Day < updatedDate.Day)
            {
                var createdDate = await CreateDateWithMessageAsync(
                    updatedDate,
                    dto.Message,
                    dto.GroupName,
                    username,
                    dto.AvatarBgColor
                );

                if (createdDate is null)
                {
                    _logger.LogError("Error al enviar mensaje para otro Date para groupName " +
                        "{groupName} por el usuario {username}", dto.GroupName, username);
                    response.IsTransactionError = true;
                    return response;
                }

                response.MessagesDate = createdDate;
                return response;

            }

            /* Simplemente agregamos un nuevo Message al chat */
            var newMessage = new Message
            {
                Content = dto.Message,
                DateId = currentMessagesDate.Id,
                Id = Guid.NewGuid().ToString(),
                MessageTime = updatedDate.TimeOfDay,
                Sender = username,
                AvatarBgColor = dto.AvatarBgColor
            };

            await _messagesService.AddMessageAsync(newMessage, null);

            response.Message = newMessage;
            _logger.LogInformation("Final correcto de envío de Message para {groupName} por usuario {username}", dto.GroupName, username);
            return response;
            
        }
        private async Task<MessagesDate?> CreateDateWithMessageAsync(DateTimeOffset updatedDate, string message, string groupName, string username, string avatarBgColor)
        {
            string dateId = Guid.NewGuid().ToString();
            var time = updatedDate.TimeOfDay;

            var messagesDate = new MessagesDate
            {
                GroupName = groupName,
                Id = dateId,
                Messages = [],
                MessagesFullDateInfo = updatedDate.Date
            };

            var newMessage = new Message
            {
                Content = message,
                DateId = dateId,
                Id = Guid.NewGuid().ToString(),
                MessageTime = time,
                Sender = username,
                AvatarBgColor = avatarBgColor
            };

            messagesDate.Messages = [newMessage];

            using var transaction = await _mongoClient.StartSessionAsync();
            transaction.StartTransaction();
            try
            {
                _logger.LogInformation("Inicio de creación de Date con Message para el groupName {groupName}", groupName);

                var newDate_Task = _repo.AddMessagesDateAsync(messagesDate, transaction);
                var newMessage_Task = _messagesRepository.AddMessageAsync(newMessage, transaction);

                await Task.WhenAll(newDate_Task, newMessage_Task);

                await transaction.CommitTransactionAsync();

                _logger.LogInformation("Crear Date con Message ejecutado correctamente para groupName {groupName}", groupName);
                return messagesDate;

            }
            catch (Exception ex)
            {
                await transaction.AbortTransactionAsync();
                _logger.LogError("Error al crear Date con Message para el grupo {groupName} por " +
                    "parte del usuario {username}. Excepción: {Msg}\nStackTrace: {StackTrace}"
                    , groupName, username, ex.Message, ex.StackTrace);

                return null;
            }
        }
    }
}
