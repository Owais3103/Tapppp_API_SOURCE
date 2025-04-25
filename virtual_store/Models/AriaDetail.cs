using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class AriaDetail
{
    public int AriaId { get; set; }

    public string? AreaName { get; set; }

    public int? Price { get; set; }

    public int? StoreId { get; set; }

    public DateTime? TransactionDt { get; set; }

    public string? EColumn { get; set; }
}
