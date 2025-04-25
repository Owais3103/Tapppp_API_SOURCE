using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class WebType
{
    public int WebId { get; set; }

    public string? WebTypeName { get; set; }

    public string? WebTypeDesc { get; set; }

    public string? WebTypeImg { get; set; }

    public DateTime? TransactionDt { get; set; }

    public string? AddedBy { get; set; }
}
