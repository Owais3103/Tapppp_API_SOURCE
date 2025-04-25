using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerContact { get; set; }

    public string? CustomerAddress { get; set; }

    public int AddedBy { get; set; }

    public DateTime? TransactionDt { get; set; }

    public int? StoreId { get; set; }

    public string? City { get; set; }

    public int? CityId { get; set; }

    public int? ProvinceId { get; set; }

    public int? CountryId { get; set; }

    public string? CustomerAria { get; set; }

    public virtual ICollection<OrderMaster> OrderMasters { get; set; } = new List<OrderMaster>();
}
