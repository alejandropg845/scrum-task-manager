using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Common.Documents
{
    public class SprintRetrospective
    {
        public string Id { get; set; }
        public string SprintId { get; set; }
        public string Name { get; set; }
        public string GroupName {  get; set; }
        public int Rating { get; set; }
        public string Feedback { get; set; }
        public DateTimeOffset SubmitedAt { get; set; }
    }
}
