using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public string? CategoryImg { get; set; }

    public string? CategoryDesc { get; set; }

    public DateTime? TransactionDt { get; set; }

    public int? StoreId { get; set; }

    public int? SequenceOrder { get; set; }
}
