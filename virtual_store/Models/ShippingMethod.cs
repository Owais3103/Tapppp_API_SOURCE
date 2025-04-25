using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class ShippingMethod
{
    public int ShippingMethodId { get; set; }

    public string MethodName { get; set; } = null!;

    public int? StoreId { get; set; }

    public decimal Price { get; set; }

    public virtual ICollection<Zonedetailwithprice> Zonedetailwithprices { get; set; } = new List<Zonedetailwithprice>();
}
