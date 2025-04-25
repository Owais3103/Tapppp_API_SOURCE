using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class WeightDetail
{
    public int WeightId { get; set; }

    public double? WeightRange { get; set; }

    public int WeightPrice { get; set; }

    public string? EColumn { get; set; }
}
