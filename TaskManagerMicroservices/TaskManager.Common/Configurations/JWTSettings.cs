using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Common.Configurations
{
    public class JWTSettings
    {
        public string Audience { get; init; }
        public string Issuer { get; init; }
        public string SigningKey { get; init; }
    }
}
