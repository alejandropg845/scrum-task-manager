using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskManager.Common.DTOs
{
    public record RequestInfo
    (
        [Required] string Method,
        [Required] string Microservice,
        [Required] string Endpoint,
        [Required] string Username,
        JsonDocument? JsonBody
    );
}
