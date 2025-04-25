using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace virtual_store.settingclasses
{
    public class CartItem
    {
        public string Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public string Image { get; set; }

        [JsonProperty("selectedVariants")]
        public Dictionary<string, string[]>? SelectedVariants { get; set; }
        public int? Category { get; set; }
        public int weight_id { get; set; }
        public int weight_price { get; set; }
        public double? weight_range{ get; set; }
        public bool IsAddon { get; set; }
 
    }

    public class CustomerDetails
    {
        public string FirstName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Contact { get; set; }
        public string customer_aria { get; set; }

        public int city_id { get; set; }

        public int country_id{ get; set; }
        public int province_id{ get; set; }
    }

    //public class ShippingMethodDetails
    //{
    //    public int ShippingMethodId { get; set; }
    //    public string MethodName { get; set; }
    //    public int StoreId { get; set; }
    //    public decimal Price { get; set; }
    //    public string? Store { get; set; }
    //}

    public class PaymentMethodDetails
    {
        public int PaymentId { get; set; }
        public string? PaymentMethodName { get; set; }
        public decimal? Price { get; set; }
    }

    public class ApiDataModel
    {
        public List<CartItem> CartItems { get; set; }
        public CustomerDetails CustomerDetails { get; set; }
        public string customer_instructions { get; set; }

        public int ShippingCost { get; set; }
   
        public int? item_price { get; set; }
        public int? storeId { get; set; }
        public decimal TotalPayment { get; set; }
        public string Discount { get; set; }
        public string? sales_tax { get; set; }

        public string? order_type { get; set; }
       // public ShippingMethodDetails? ShippingMethodDetails { get; set; }
        public string PaymentMethodDetails { get; set; }
        public int totalWeight { get; set; } 
    }

}
