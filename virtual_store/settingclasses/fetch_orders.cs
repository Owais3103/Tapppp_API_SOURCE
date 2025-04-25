using virtual_store.Models;

namespace virtual_store.settingclasses
{

    public  class gro
    {
        public OrderMaster orderMaster { get; set; }
        public int discount{ get; set; }
    }



    public partial class OrderMaster_Fetch
    {
        public int Mid { get; set; }

        public string? OrderId { get; set; }

        public int? CustomerId { get; set; }

        public int? Quantity { get; set; }

        public string? TotalPrice { get; set; }

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

        public virtual Customer? Customer { get; set; }

        public virtual ICollection<OrderDetail_Fetch> OrderDetails { get; set; } = new List<OrderDetail_Fetch>();

        public virtual PaymentMethod? PaymentMethodNavigation { get; set; }
    }


    public partial class OrderDetail_Fetch
    {
        public int Did { get; set; }

        public int? Mid { get; set; }

        public int? ProductId { get; set; }

        public int? ProductItem { get; set; }

        public int? Price { get; set; }
        public string? Product_img { get; set; }
        public string? EColumn { get; set; }

        public DateTime? TransactionDt { get; set; }

        public int? AddedBy { get; set; }

        public int? StoreId { get; set; }

        public virtual OrderMaster_Fetch? MidNavigation { get; set; }

        public virtual ICollection<OrderDetailSubDetail> OrderDetailSubDetails { get; set; } = new List<OrderDetailSubDetail>();

        public virtual Product? Product { get; set; }
    }

}

