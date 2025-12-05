using Microsoft.AspNetCore.Http;
using RentCarX.Domain.Interfaces.UserContext;
using System.Security.Claims;

namespace RentCarX.Application.Services.User
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _accessor;

        public UserContextService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public Guid UserId =>
            Guid.Parse(_accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new Exception("No user"));

        public string Email =>
            _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)?.ToString()
                ?? throw new Exception("No user's email found");
    }

}
