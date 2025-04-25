using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class PaymentMethod
{
    public int Paymentid { get; set; }

    public string? PaymentMethodName { get; set; }

    public int? Price { get; set; }
}
