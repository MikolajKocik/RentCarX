using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Domain.Exceptions
{
    public class ConflictException(string resourceType, string resourceIdentifier)
       : Exception($"{resourceType} with provided object: {resourceIdentifier} already exists");
}
