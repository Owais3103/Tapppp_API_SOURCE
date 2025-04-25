using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class LoginHistory
{
    public int Logid { get; set; }

    public string? LogName { get; set; }

    public string? Userid { get; set; }

    public string? Region { get; set; }

    public DateTime? TransactionDt { get; set; }

    public string? StoreId { get; set; }
}
