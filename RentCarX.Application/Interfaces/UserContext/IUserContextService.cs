using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Domain.Interfaces.UserContext
{
    public interface IUserContextService
    {
        Guid UserId { get; }
    }
}
