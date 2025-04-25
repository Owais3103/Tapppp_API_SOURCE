using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? ProductDesc { get; set; }

    public int? StoreId { get; set; }

    public string? ProductImg { get; set; }

    public int? CategoryId { get; set; }

    public int? ProductPrice { get; set; }

    public int? AddedBy { get; set; }

    public string? EColumn { get; set; }

    public DateTime? TransactionDt { get; set; }

    public decimal? SalesTax { get; set; }

    public int? Discount { get; set; }

    public long? Stock { get; set; }

    public int? WeightId { get; set; }

    public string? Isactive { get; set; }

    public int? IsLimitedSelection { get; set; }

    public int? NumberOfSelection { get; set; }

    public int? SequenceOrder { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
