using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class City
{
    public int CityId { get; set; }

    public string? CityName { get; set; }

    public int? ProvinceId { get; set; }

    public string? EColumn { get; set; }
}
