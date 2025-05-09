namespace RentCarX.Domain.Exceptions
{
    public class UnauthorizedException(string resourceType)
   : Exception($"Unauthorized resource: {resourceType}");
}
