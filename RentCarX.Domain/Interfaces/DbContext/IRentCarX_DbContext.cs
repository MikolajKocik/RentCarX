using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RentCarX.Domain.Models;

namespace RentCarX.Domain.Interfaces.DbContext
{
    public interface IRentCarX_DbContext
    {
        DatabaseFacade Database { get; }
    }
}
