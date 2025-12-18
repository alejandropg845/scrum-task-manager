using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Common.Documents
{
    public class MessagesDate
    {
        public string Id { get; set; }
        public string GroupName { get; set; }
        public DateTime MessagesFullDateInfo { get; set; }
        public List<Message> Messages { get; set; }
    }
}
