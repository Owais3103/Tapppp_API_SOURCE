using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class OrderDetail
{
    public int Did { get; set; }

    public int? Mid { get; set; }

    public int? ProductId { get; set; }

    public int? ProductItem { get; set; }

    public int? Price { get; set; }

    public string? EColumn { get; set; }

    public DateTime? TransactionDt { get; set; }

    public int? AddedBy { get; set; }

    public int? StoreId { get; set; }

    public string? ProductImage { get; set; }

    public int? WeightId { get; set; }

    public string? WeightRange { get; set; }

    public int? WeightPrice { get; set; }

    public virtual OrderMaster? MidNavigation { get; set; }

    public virtual ICollection<OrderDetailSubDetail> OrderDetailSubDetails { get; set; } = new List<OrderDetailSubDetail>();

    public virtual Product? Product { get; set; }
}
