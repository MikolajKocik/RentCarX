using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Infrastructure.Helpers.Development
{
    internal static class ConnectionStringValidation
    {
        internal static void CheckParameters(params string[] databaseParameters)
        {
            foreach (string parameter in databaseParameters)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(parameter);
            };
        }
    }
}
