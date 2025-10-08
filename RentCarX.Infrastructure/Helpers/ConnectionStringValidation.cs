using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Infrastructure.Helpers
{
    public static class ConnectionStringValidation
    {
        public static void CheckParameters(params string[] databaseParameters)
        {
            foreach (string parameter in databaseParameters)
            {
                ArgumentNullException.ThrowIfNullOrWhiteSpace(parameter);
            };
        }
    }
}
