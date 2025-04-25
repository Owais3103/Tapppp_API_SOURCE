using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class PayDetail
{
    public int PaydetailId { get; set; }

    public int? WithinCityPrice { get; set; }

    public int? OutOfCityPrice { get; set; }

    public int? Paymentid { get; set; }
}
