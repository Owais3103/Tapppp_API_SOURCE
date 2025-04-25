using virtual_store.Models;

namespace virtual_store.settingclasses
{
    public class Admin_Customers
    {
    }
    public class CustomerDTO
    {
        public string CustomerName { get; set; }
        public int ProvinceId { get; set; }
        public int? CityId { get; set; }
        public int? store_id { get; set; }

        public List<OrderMaster> OrderMasters { get; set; }
    }

    public class OrderMasterDTO
    {
        public int OrderId { get; set; }
        public int TotalPrice { get; set; }
    }

}
