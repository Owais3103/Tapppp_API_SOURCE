using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class VariantValue
{
    public int VariantValueId { get; set; }

    public int? VariantTypeId { get; set; }

    public string? Value { get; set; }

    public int? StoreId { get; set; }

    public string? VariantImg { get; set; }

    public int? VariantPrice { get; set; }

    public int? ProductId { get; set; }

    public int? VariantStock { get; set; }

    public virtual VariantType? VariantType { get; set; }
}
