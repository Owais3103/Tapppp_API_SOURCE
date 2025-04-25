using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class PickupDetail
{
    public int PickupId { get; set; }

    public string? PickupLocation { get; set; }

    public int? StoreId { get; set; }

    public string? EColumn { get; set; }

    public DateTime? TransactionDt { get; set; }
}
