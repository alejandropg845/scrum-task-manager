using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Common.Configurations
{
    public class Microservices
    {
        public string Users { get; init; }
        public string TaskItems { get; init; }
        public string Tasks { get; init; }
        public string Sprints { get;init; }
        public string GroupsRoles { get; init;}
        public string Groups { get; init; }
        public string Chats { get; init; }
        public string Tokens { get; set; }
    }
}
