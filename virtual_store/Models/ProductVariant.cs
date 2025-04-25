using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class ProductVariant
{
    public int ProductVariantId { get; set; }

    public int? ProductId { get; set; }

    public int? StoreId { get; set; }

    public int? VariantValueId { get; set; }
}
