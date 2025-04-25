using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class OrderSerial
{
    public int SeriesId { get; set; }

    public string? SeriesCode { get; set; }

    public int? Storeid { get; set; }
}
