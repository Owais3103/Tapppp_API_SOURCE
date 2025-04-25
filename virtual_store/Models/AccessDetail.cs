using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class AccessDetail
{
    public int AccessId { get; set; }

    public string? AccessLink { get; set; }

    public int? StoreId { get; set; }

    public string? AccessStatus { get; set; }
}
