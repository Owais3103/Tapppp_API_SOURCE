using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class Zonedetailwithprice
{
    public int ZoneId { get; set; }

    public string? CityName { get; set; }

    public string? ZoneName { get; set; }

    public int? Price { get; set; }

    public int? StoreId { get; set; }

    public int? ShipmentId { get; set; }

    public virtual ShippingMethod? Shipment { get; set; }
}
