using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Common.Documents
{
    public class Feedback
    {
        public string Id { get; set; }
        public string GroupName { get; set; }
        public string SprintId { get; set; }
        public string Username { get;set; }
        public bool IsSubmited { get; set; }
    }
}
