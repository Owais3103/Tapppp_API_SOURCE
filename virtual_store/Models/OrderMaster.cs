using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class OrderMaster
{
    public int Mid { get; set; }

    public string? OrderId { get; set; }

    public int? CustomerId { get; set; }

    public int? Quantity { get; set; }

    public int? TotalPrice { get; set; }

    public string? OrderStatus { get; set; }

    public DateTime? TransactionDt { get; set; }

    public string? EColumn { get; set; }

    public string? AddedBy { get; set; }

    public int? StoreId { get; set; }

    public int? IsCancel { get; set; }

    public string? CancelReason { get; set; }

    public int? IsApproved { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public int? PaymentMethod { get; set; }

    public int? ItemPrice { get; set; }

    public int? IsSeen { get; set; }

    public DateTime? LastUpdateDate { get; set; }

    public int? ShippingPrice { get; set; }

    public string? PaymentMethodName { get; set; }

    public int? Discount { get; set; }

    public int? CityId { get; set; }

    public int? ProvinceId { get; set; }

    public int? CountryId { get; set; }

    public int? WeightId { get; set; }

    public int? WeightPrice { get; set; }

    public double? TotalWeight { get; set; }

    public int? OmsOrderId { get; set; }

    public string? OrderType { get; set; }

    public string? CustomerInstruction { get; set; }

    public double? SalesTax { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
