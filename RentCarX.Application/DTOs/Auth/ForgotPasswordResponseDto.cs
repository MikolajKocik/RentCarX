using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.DTOs.Auth
{
    public sealed record ForgotPasswordResponseDto
    {
        public required string ResetLink { get; init; } 
    }
}
