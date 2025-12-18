using TaskManager.Common.Documents;

namespace TaskManager.Chats
{
    public class SendMessageResponse
    {
        public MessagesDate MessagesDate { get; set; }
        public Message Message { get; set; }
        public bool IsTransactionError { get; set; }
    }
    public class GetMessagesResponse
    {
        public MessagesDate MessagesDate { get; set; }
        public List<Message> Messages { get; set; }
        public bool NoMoreMessages { get; set; }
        public bool NoMoreDates { get; set; }
        public string DateId { get; set; }
    }
}
