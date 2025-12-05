using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.DTOs.Auth
{
    public sealed record RegisterUserResponseDto
    {
        public required string JwtToken { get; init; } 
        public Guid UserId { get; set; } 
        public required string ConfirmationLink { get; init; } 
    }
}
