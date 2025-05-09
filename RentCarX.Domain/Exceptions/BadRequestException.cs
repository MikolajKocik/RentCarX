namespace RentCarX.Domain.Exceptions
{
    public class BadRequestException(string resourceType, string resourceIdentifier)
      : Exception($"{resourceType} with id: {resourceIdentifier} is not valid");
}
