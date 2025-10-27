using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Domain.ExceptionModels;

public sealed class AlreadyDeletedException : Exception
{
    public AlreadyDeletedException(string message, string source) : base(message) { }
    public AlreadyDeletedException(string message) : base(message) { }
}
