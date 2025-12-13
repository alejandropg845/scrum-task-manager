using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Common.Documents
{
    public class Message
    {
        public string Id { get; set; }
        public string DateId { get; set; }
        public string Content { get; set; }
        public string Sender { get; set; }
        public string AvatarBgColor { get; set; }
        public TimeSpan MessageTime {  get; set; }
    }
}
