using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentCarX.Infrastructure.Settings;

public sealed class AzureSqlConnection
{
    public string DataSource { get; set; } = string.Empty;
    public string UserID { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string InitialCatalog { get; set; } = string.Empty;
}
