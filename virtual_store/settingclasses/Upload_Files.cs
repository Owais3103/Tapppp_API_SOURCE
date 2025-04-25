using Org.BouncyCastle.Crypto.Engines;
using virtual_store.Models;

namespace virtual_store.settingclasses
{
    public class Upload_Files
    {
        public List<AriaDetail>? Area_Details { get; set; }
        public List<PickupDetail>? Pickup_Details { get; set; }
        public int store_id { get; set; }
    }
}
