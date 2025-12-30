namespace RentCarX.Domain.ExceptionModels;

public class ForbiddenException : Exception
{
    public string ResourceType { get; }
    public string ResourceIdentifier { get; }

    public ForbiddenException(string resourceType, string resourceIdentifier)
        : base($"{resourceType} with identifier {resourceIdentifier} not found.")
    {
        ResourceType = resourceType;
        ResourceIdentifier = resourceIdentifier;
    }
}
