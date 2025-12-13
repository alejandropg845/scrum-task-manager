using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Common.Documents
{
    public class Token
    {
        public string Id { get; set; }
        public string RefreshToken { get; set; }
        public string Username { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }
}
