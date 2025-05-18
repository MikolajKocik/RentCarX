using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.DTOs.Auth
{
    public class RegisterUserResponseDto
    {
        public string JwtToken { get; set; } 
        public Guid UserId { get; set; } 
        public string ConfirmationLink { get; set; } 
    }
}
