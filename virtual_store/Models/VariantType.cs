using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class VariantType
{
    public int VariantTypeId { get; set; }

    public int? StoreId { get; set; }

    public string Title { get; set; } = null!;

    public string? SelectionMode { get; set; }

    public int? ProductId { get; set; }

    public int? IsVariantLimitSelection { get; set; }

    public int? VariantNoOfSelection { get; set; }

    public virtual ICollection<VariantValue> VariantValues { get; set; } = new List<VariantValue>();
}
