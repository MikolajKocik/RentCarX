using RentCarX.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Application.CQRS.Commands.Auth.Login
{
    public record LoginUserCommand(LoginDto Dto) : IRequest<string>;

}
