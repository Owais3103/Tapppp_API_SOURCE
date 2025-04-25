using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class OrderDetailSubDetail
{
    public int SubDetailId { get; set; }

    public int Did { get; set; }

    public string AttributeName { get; set; } = null!;

    public string AttributeValue { get; set; } = null!;

    public int? StoreId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public virtual OrderDetail DidNavigation { get; set; } = null!;
}
