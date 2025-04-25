using virtual_store.Models;

namespace virtual_store.settingclasses
{
    public class Product_Add
    {
        public string name { get; set; }
        public string description { get; set; }
        public string price { get; set; }
        public string sales_tax { get; set; }
        public string discount { get; set; }
        public string image { get; set; }
        public int category_id { get; set; }
        //   public string category { get; set; }
        public int? WeightId { get; set; }

        public int? IsLimitedSelection { get; set; }

        public int? NumberOfSelection { get; set; }

        public int? WeightPrice { get; set; }

        public string? WeightRange { get; set; }
        public Dictionary<string, List<Dropdown>> Dropdowns { get; set; }
        public Dictionary<string, List<Addon>> Addons { get; set; }
        public virtual ICollection<ProductsImage> MultipleImges { get; set; } = new List<ProductsImage>();
        public int store_id { get; set; }
    }


    public class Dropdownss
    {
        public int VariantTypeId { get; set; }

        public int? StoreId { get; set; }

        public int? IsVariantLimitSelection { get; set; }

        public int? VariantNoOfSelection { get; set; }
        public string Title { get; set; } = null!;

        public string? SelectionMode { get; set; }

        public virtual Store? Store { get; set; }
    }



    public class Dropdown
    {
        public string Name { get; set; }
        public int Price { get; set; }
        public int? IsVariantLimitSelection { get; set; }

        public int? VariantNoOfSelection { get; set; }
        public string Image { get; set; } // Use object if the image could be null or other data type
    }

    public class Addon
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }




}
