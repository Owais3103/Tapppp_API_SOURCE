using System;
using System.Collections.Generic;

namespace virtual_store.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? UserName { get; set; }

    public int? StoreId { get; set; }

    public string? Region { get; set; }

    public string? UserContact { get; set; }

    public string? Useremail { get; set; }

    public string? UserPassword { get; set; }

    public string? UserRole { get; set; }

    public string? Isactive { get; set; }

    public DateTime? TransactionDt { get; set; }
}
