namespace virtual_store.settingclasses
{
    public class Edit_Store
    {
        public int store_id  { get; set; }
        public string? businessName { get; set; }
   
            public string? cnic { get; set; }
            public string? phone { get; set; }
            public string? email { get; set; }
            public string? owner_name { get; set; }
            public string? oldPassword { get; set; }
            public string? newPassword { get; set; }
        public string? username { get; set; }
        public string? confirmPassword { get; set; }
            public string?   facebookUrl { get; set; }
            public string? instagramUrl { get; set; }
            public string? linkedInUrl { get; set; }
            public string? whatsappUrl { get; set; }
            public string? marketingTagline { get; set; }
            public string? text_color { get; set; }
            public string? background_color { get; set; }
        public string? facebookpixel { get; set; }
        public string? store_location { get; set; }
        public string? pickup_location { get; set; }

        public string[] productImages { get; set; }
            public string? logo { get; set; }
        public int? FreeDeliveryAmount { get; set; }

        public int? IsOpen { get; set; }

        public string? ButtonColor { get; set; }

        public double? SalesTax { get; set; }

    }
}
