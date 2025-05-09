namespace RentCarX.Domain.Exceptions
{
    public class ConflictException(string resourceType, string resourceIdentifier)
       : Exception($"{resourceType} with provided object: {resourceIdentifier} already exists");
}
