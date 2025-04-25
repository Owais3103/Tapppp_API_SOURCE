using virtual_store.Models;

namespace virtual_store.settingclasses
{
    public class ship_pay
    {
        public List< ShippingMethod>  shipping { get; set; }
        public List< PaymentMethod>  payment{ get; set; }
    }
}
