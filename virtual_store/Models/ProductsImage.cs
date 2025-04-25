using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class ProductsImage
{
    public int ImageId { get; set; }

    public string? ImageUrl { get; set; }

    public int? ProductId { get; set; }

    public int? StoreId { get; set; }

    public int? IsActive { get; set; }
}
