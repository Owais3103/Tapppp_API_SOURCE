using System.Text.Json.Serialization;
using virtual_store.Models;

namespace virtual_store.settingclasses
{


    public class Product_AddWithDropdown
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Price { get; set; }
        public string SalesTax { get; set; }
        public string Discount { get; set; }
        public string EColumn { get; set; }
        public string Image { get; set; }

        public int? IsLimitedSelection { get; set; }

        public int? NumberOfSelection { get; set; }

        public virtual ICollection<ProductsImage> MultipleImges { get; set; } = new List<ProductsImage>();
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public int StoreId { get; set; }
        public int so { get; set; }
        public string IsActive { get; set; }

        // Dropdowns for selecting VariantType
        public List<Dropdown_Fetch> VariantTypeDropdowns { get; set; } // Dropdown list for VariantTypes

            public int? WeightId { get; set; }

    public int? WeightPrice { get; set; }

    public double? WeightRange { get; set; }
        // Addons for each product (e.g., variants selected)
        public List<Addon_Fetch> Addons { get; set; }
        public List<Product> Addons_Product { get; set; }
    }

    public class Dropdown_Fetch
    {
        public int VariantTypeId { get; set; }
        public int ProductId { get; set; }
        public string Title { get; set; }
        public int? IsVariantLimitSelection { get; set; }

        public int? VariantNoOfSelection { get; set; }
        public List<DropdownOption> Options { get; set; } // Options for the variant type
    }

    public class Addon_Fetch
    {

        public int VariantTypeId { get; set; }
        public int ProductId { get; set; }
        public string Title { get; set; }
        public List<DropdownOptionp> Options { get; set; }
        public List<Product_Addon> Products { get; set; }

    }


    public class DropdownOptionp
    {
        //   public int VariantValueId { get; set; }

        public List<ProductDetail> Products { get; set; }
    }


    public class DropdownOption
    {
        public int VariantValueId { get; set; }
        public string Value { get; set; }
        public string VariantImg { get; set; }
        public int productIds { get; set; }
        public decimal? VariantPrice { get; set; }
    }




    public class ProductDetail
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Price { get; set; }

        public string Image { get; set; }
    }


    public partial class Product_Addon
    {
        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        public string? ProductDesc { get; set; }

        public int? StoreId { get; set; }

        public string? ProductImg { get; set; }

        public int? CategoryId { get; set; }

        public int? ProductPrice { get; set; }

        public int? AddedBy { get; set; }

        public string? EColumn { get; set; }
        public string? isactive { get; set; }

        public DateTime? TransactionDt { get; set; }

        public decimal? SalesTax { get; set; }

        public int? Discount { get; set; }

        public int? WeightId { get; set; }

        public int? WeightPrice { get; set; }

        public double? WeightRange { get; set; }
        public long? Stock { get; set; }



    }


}
