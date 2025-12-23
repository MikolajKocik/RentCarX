using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.DTOs.Auth;

public sealed class UserDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; } 
    public DateTime? DeletedAt { get; set; }

}

