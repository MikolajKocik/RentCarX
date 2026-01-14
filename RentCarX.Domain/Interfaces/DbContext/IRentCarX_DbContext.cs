using Microsoft.EntityFrameworkCore.Storage;

namespace RentCarX.Domain.Interfaces.DbContext;

public interface IRentCarX_DbContext
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
