using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace virtual_store.Models;

public partial class VirtualStoreContext : DbContext
{
    public VirtualStoreContext()
    {
    }

    public VirtualStoreContext(DbContextOptions<VirtualStoreContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessDetail> AccessDetails { get; set; }

    public virtual DbSet<AriaDetail> AriaDetails { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<LoginHistory> LoginHistories { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<OrderDetailSubDetail> OrderDetailSubDetails { get; set; }

    public virtual DbSet<OrderMaster> OrderMasters { get; set; }

    public virtual DbSet<OrderSerial> OrderSerials { get; set; }

    public virtual DbSet<PayDetail> PayDetails { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PickupDetail> PickupDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<ProductsImage> ProductsImages { get; set; }

    public virtual DbSet<ProductsImagesByVariantId> ProductsImagesByVariantIds { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VariantType> VariantTypes { get; set; }

    public virtual DbSet<VariantValue> VariantValues { get; set; }

    public virtual DbSet<WebType> WebTypes { get; set; }

    public virtual DbSet<WeightDetail> WeightDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=66.165.248.146;Database=tapppp_store_db;User Id=tapppp_dbs;Password=Tapppp24;Trusted_Connection=False;MultipleActiveResultSets=false;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessDetail>(entity =>
        {
            entity.HasKey(e => e.AccessId);

            entity.ToTable("Access_Detail");

            entity.Property(e => e.AccessId).HasColumnName("access_id");
            entity.Property(e => e.AccessLink)
                .HasMaxLength(450)
                .HasColumnName("access_link");
            entity.Property(e => e.AccessStatus)
                .HasMaxLength(50)
                .HasColumnName("access_status");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
        });

        modelBuilder.Entity<AriaDetail>(entity =>
        {
            entity.HasKey(e => e.AriaId);

            entity.ToTable("Aria_Detail");

            entity.Property(e => e.AriaId).HasColumnName("aria_id");
            entity.Property(e => e.AreaName).HasColumnName("Area_Name");
            entity.Property(e => e.EColumn).HasColumnName("e_column");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.TransactionDt)
                .HasColumnType("datetime")
                .HasColumnName("transaction_dt");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryDesc)
                .HasMaxLength(500)
                .HasColumnName("category_desc");
            entity.Property(e => e.CategoryImg)
                .HasMaxLength(500)
                .HasColumnName("category_img");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(150)
                .HasColumnName("category_name");
            entity.Property(e => e.SequenceOrder).HasColumnName("sequence_order");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.TransactionDt)
                .HasColumnType("datetime")
                .HasColumnName("transaction_dt");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.CityName)
                .HasMaxLength(250)
                .HasColumnName("city_name");
            entity.Property(e => e.EColumn)
                .HasMaxLength(350)
                .HasColumnName("e_column");
            entity.Property(e => e.ProvinceId).HasColumnName("province_id");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK_Customers_1");

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.AddedBy).HasColumnName("added_by");
            entity.Property(e => e.City).HasMaxLength(250);
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.CustomerAddress)
                .HasMaxLength(250)
                .HasColumnName("customer_address");
            entity.Property(e => e.CustomerAria).HasColumnName("customer_aria");
            entity.Property(e => e.CustomerContact)
                .HasMaxLength(50)
                .HasColumnName("customer_contact");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(250)
                .HasColumnName("customer_name");
            entity.Property(e => e.ProvinceId).HasColumnName("province_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.TransactionDt)
                .HasColumnType("datetime")
                .HasColumnName("transaction_dt");
        });

        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.HasKey(e => e.Logid);

            entity.ToTable("Login_History");

            entity.Property(e => e.Logid).HasColumnName("logid");
            entity.Property(e => e.LogName)
                .HasMaxLength(150)
                .HasColumnName("log_name");
            entity.Property(e => e.Region)
                .HasMaxLength(50)
                .HasColumnName("region");
            entity.Property(e => e.StoreId)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("store_id");
            entity.Property(e => e.TransactionDt)
                .HasColumnType("datetime")
                .HasColumnName("transaction_dt");
            entity.Property(e => e.Userid)
                .HasMaxLength(50)
                .HasColumnName("userid");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Did);

            entity.ToTable("Order_Detail");

            entity.Property(e => e.Did).HasColumnName("DID");
            entity.Property(e => e.AddedBy).HasColumnName("added_by");
            entity.Property(e => e.EColumn)
                .HasMaxLength(50)
                .HasColumnName("e_column");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductImage)
                .HasMaxLength(350)
                .HasColumnName("product_image");
            entity.Property(e => e.ProductItem).HasColumnName("product_item");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.TransactionDt)
                .HasColumnType("datetime")
                .HasColumnName("transaction_dt");
            entity.Property(e => e.WeightId).HasColumnName("weight_id");
            entity.Property(e => e.WeightPrice).HasColumnName("weight_price");
            entity.Property(e => e.WeightRange)
                .HasMaxLength(150)
                .HasColumnName("weight_range");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_Order_Detail_Order_Master");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_Order_Detail_Products");
        });

        modelBuilder.Entity<OrderDetailSubDetail>(entity =>
        {
            entity.HasKey(e => e.SubDetailId).HasName("PK__Order_De__D5A06649FF89298E");

            entity.ToTable("Order_Detail_Sub_Detail");

            entity.Property(e => e.SubDetailId).HasColumnName("SubDetailID");
            entity.Property(e => e.AttributeName).HasMaxLength(100);
            entity.Property(e => e.AttributeValue).HasMaxLength(655);
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("created_by");
            entity.Property(e => e.Did).HasColumnName("DID");
            entity.Property(e => e.StoreId).HasColumnName("store_id");

            entity.HasOne(d => d.DidNavigation).WithMany(p => p.OrderDetailSubDetails)
                .HasForeignKey(d => d.Did)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Detail_Sub_Detail_Order_Detail");
        });

        modelBuilder.Entity<OrderMaster>(entity =>
        {
            entity.HasKey(e => e.Mid);

            entity.ToTable("Order_Master");

            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.AddedBy)
                .HasMaxLength(50)
                .HasColumnName("added_by");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .HasColumnName("address");
            entity.Property(e => e.CancelReason)
                .HasMaxLength(350)
                .HasColumnName("cancel_reason");
            entity.Property(e => e.City)
                .HasMaxLength(150)
                .HasColumnName("city");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.CustomerInstruction).HasColumnName("customer_instruction");
            entity.Property(e => e.Discount).HasColumnName("discount");
            entity.Property(e => e.EColumn)
                .HasMaxLength(50)
                .HasColumnName("e_column");
            entity.Property(e => e.IsSeen).HasColumnName("is_seen");
            entity.Property(e => e.ItemPrice).HasColumnName("item_price");
            entity.Property(e => e.LastUpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("last_update_date");
            entity.Property(e => e.OmsOrderId).HasColumnName("OMS_order_id");
            entity.Property(e => e.OrderId)
                .HasMaxLength(150)
                .HasColumnName("order_id");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(50)
                .HasColumnName("order_status");
            entity.Property(e => e.OrderType)
                .HasMaxLength(150)
                .HasColumnName("order_type");
            entity.Property(e => e.PaymentMethod).HasColumnName("paymentMethod");
            entity.Property(e => e.PaymentMethodName)
                .HasMaxLength(150)
                .HasColumnName("payment_method_name");
            entity.Property(e => e.ProvinceId).HasColumnName("province_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SalesTax).HasColumnName("sales_tax");
            entity.Property(e => e.ShippingPrice).HasColumnName("shipping_price");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price");
            entity.Property(e => e.TotalWeight).HasColumnName("totalWeight");
            entity.Property(e => e.TransactionDt)
                .HasColumnType("datetime")
                .HasColumnName("transaction_dt");
            entity.Property(e => e.WeightId).HasColumnName("weight_id");
            entity.Property(e => e.WeightPrice).HasColumnName("weight_price");

            entity.HasOne(d => d.Customer).WithMany(p => p.OrderMasters)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Order_Master_Customers");
        });

        modelBuilder.Entity<OrderSerial>(entity =>
        {
            entity.HasKey(e => e.SeriesId);

            entity.ToTable("order_serial");

            entity.Property(e => e.SeriesId).HasColumnName("series_id");
            entity.Property(e => e.SeriesCode)
                .HasMaxLength(150)
                .HasColumnName("series_code");
            entity.Property(e => e.Storeid).HasColumnName("storeid");
        });

        modelBuilder.Entity<PayDetail>(entity =>
        {
            entity.Property(e => e.PaydetailId).HasColumnName("paydetail_id");
            entity.Property(e => e.OutOfCityPrice).HasColumnName("out_of_city_price");
            entity.Property(e => e.Paymentid).HasColumnName("paymentid");
            entity.Property(e => e.WithinCityPrice).HasColumnName("within_city_price");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Paymentid);

            entity.ToTable("Payment_method");

            entity.Property(e => e.Paymentid).HasColumnName("paymentid");
            entity.Property(e => e.PaymentMethodName)
                .HasMaxLength(150)
                .HasColumnName("Payment_Method_Name");
            entity.Property(e => e.Price).HasColumnName("price");
        });

        modelBuilder.Entity<PickupDetail>(entity =>
        {
            entity.HasKey(e => e.PickupId);

            entity.ToTable("Pickup_Detail");

            entity.Property(e => e.PickupId).HasColumnName("pickup_id");
            entity.Property(e => e.EColumn)
                .HasMaxLength(350)
                .HasColumnName("e_column");
            entity.Property(e => e.PickupLocation)
                .HasMaxLength(450)
                .HasColumnName("pickup_location");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.TransactionDt)
                .HasColumnType("datetime")
                .HasColumnName("transaction_dt");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.AddedBy).HasColumnName("added_by");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Discount).HasColumnName("discount");
            entity.Property(e => e.EColumn)
                .HasMaxLength(150)
                .HasColumnName("e_column");
            entity.Property(e => e.IsLimitedSelection).HasColumnName("is_limited_selection");
            entity.Property(e => e.Isactive)
                .HasMaxLength(50)
                .HasColumnName("isactive");
            entity.Property(e => e.NumberOfSelection).HasColumnName("number_of_selection");
            entity.Property(e => e.ProductDesc).HasColumnName("product_desc");
            entity.Property(e => e.ProductImg)
                .HasMaxLength(500)
                .HasColumnName("product_img");
            entity.Property(e => e.ProductName)
                .HasMaxLength(150)
                .HasColumnName("product_name");
            entity.Property(e => e.ProductPrice).HasColumnName("product_price");
            entity.Property(e => e.SalesTax)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("sales_tax");
            entity.Property(e => e.SequenceOrder).HasColumnName("sequence_order");
            entity.Property(e => e.Stock).HasColumnName("stock");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.TransactionDt)
                .HasColumnType("datetime")
                .HasColumnName("transaction_dt");
            entity.Property(e => e.WeightId).HasColumnName("weight_id");
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.ProductVariantId).HasName("PK__ProductV__E4D6672527A9CDF7");

            entity.ToTable("ProductVariant");

            entity.Property(e => e.ProductVariantId).HasColumnName("ProductVariantID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.VariantValueId).HasColumnName("VariantValueID");
        });

        modelBuilder.Entity<ProductsImage>(entity =>
        {
            entity.HasKey(e => e.ImageId);

            entity.ToTable("Products_Images");

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
        });

        modelBuilder.Entity<ProductsImagesByVariantId>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__Products__7516F4ECE3A34873");

            entity.ToTable("ProductsImages_ByVariantID");

            entity.Property(e => e.ImageId).HasColumnName("ImageID");
            entity.Property(e => e.ImageUrl).HasColumnName("ImageURL");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.ProductVariantId).HasColumnName("ProductVariantID");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.AddedBy).HasColumnName("added_by");
            entity.Property(e => e.BackgroundColor)
                .HasMaxLength(50)
                .HasColumnName("background_color");
            entity.Property(e => e.BusinessType)
                .HasMaxLength(300)
                .HasColumnName("businessType");
            entity.Property(e => e.ButtonColor)
                .HasMaxLength(150)
                .HasColumnName("button_color");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.CommisionShipmentPrice).HasColumnName("commision_shipment_price");
            entity.Property(e => e.CountryIdId).HasColumnName("country_id_id");
            entity.Property(e => e.CoverImage).HasColumnName("cover_image");
            entity.Property(e => e.EColumn)
                .HasMaxLength(250)
                .HasColumnName("e_column");
            entity.Property(e => e.ExtraColumn)
                .HasMaxLength(500)
                .HasColumnName("extra_column");
            entity.Property(e => e.FacebookLink)
                .HasMaxLength(500)
                .HasColumnName("facebook_link");
            entity.Property(e => e.FacebookPixelLink).HasColumnName("facebook_pixel_link");
            entity.Property(e => e.FreeDeliveryAmount).HasColumnName("free_delivery_amount");
            entity.Property(e => e.InstagramLink)
                .HasMaxLength(500)
                .HasColumnName("instagram_link");
            entity.Property(e => e.IsOpen).HasColumnName("is_open");
            entity.Property(e => e.Isactive)
                .HasMaxLength(50)
                .HasColumnName("isactive");
            entity.Property(e => e.LinkedinLink)
                .HasMaxLength(500)
                .HasColumnName("linkedin_link");
            entity.Property(e => e.OwnerCnic)
                .HasMaxLength(150)
                .HasColumnName("owner_cnic");
            entity.Property(e => e.OwnerEmail)
                .HasMaxLength(150)
                .HasColumnName("owner_email");
            entity.Property(e => e.OwnerName)
                .HasMaxLength(350)
                .HasColumnName("owner_name");
            entity.Property(e => e.PickupAddress)
                .HasMaxLength(450)
                .HasColumnName("pickup_address");
            entity.Property(e => e.ProvinceId).HasColumnName("province_id");
            entity.Property(e => e.SalesTax).HasColumnName("sales_tax");
            entity.Property(e => e.ShipmentMode).HasColumnName("shipment_mode");
            entity.Property(e => e.StoreCity)
                .HasMaxLength(250)
                .HasColumnName("store_city");
            entity.Property(e => e.StoreContact)
                .HasMaxLength(50)
                .HasColumnName("store_contact");
            entity.Property(e => e.StoreImg)
                .HasMaxLength(500)
                .HasColumnName("store_img");
            entity.Property(e => e.StoreLink)
                .HasMaxLength(520)
                .HasColumnName("store_link");
            entity.Property(e => e.StoreLocation)
                .HasMaxLength(350)
                .HasColumnName("store_location");
            entity.Property(e => e.StoreName)
                .HasMaxLength(250)
                .HasColumnName("store_name");
            entity.Property(e => e.TagLine)
                .HasMaxLength(740)
                .HasColumnName("tag_line");
            entity.Property(e => e.TextColor)
                .HasMaxLength(50)
                .HasColumnName("text_color");
            entity.Property(e => e.TransactionDt)
                .HasColumnType("datetime")
                .HasColumnName("transaction_dt");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WebType).HasColumnName("web_type");
            entity.Property(e => e.WhatsappLink)
                .HasMaxLength(500)
                .HasColumnName("whatsapp_link");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Isactive)
                .HasMaxLength(50)
                .HasColumnName("isactive");
            entity.Property(e => e.Region)
                .HasMaxLength(350)
                .HasColumnName("region");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.TransactionDt)
                .HasColumnType("datetime")
                .HasColumnName("transaction_dt");
            entity.Property(e => e.UserContact)
                .HasMaxLength(30)
                .HasColumnName("user_contact");
            entity.Property(e => e.UserName)
                .HasMaxLength(150)
                .HasColumnName("user_name");
            entity.Property(e => e.UserPassword)
                .HasMaxLength(50)
                .HasColumnName("user_password");
            entity.Property(e => e.UserRole)
                .HasMaxLength(50)
                .HasColumnName("user_role");
            entity.Property(e => e.Useremail)
                .HasMaxLength(150)
                .HasColumnName("useremail");
        });

        modelBuilder.Entity<VariantType>(entity =>
        {
            entity.HasKey(e => e.VariantTypeId).HasName("PK__VariantT__0CAD5D7C4B018CCE");

            entity.ToTable("VariantType");

            entity.Property(e => e.VariantTypeId).HasColumnName("VariantTypeID");
            entity.Property(e => e.IsVariantLimitSelection).HasColumnName("is_variant_limit_selection");
            entity.Property(e => e.ProductId).HasColumnName("Product_id");
            entity.Property(e => e.SelectionMode)
                .HasMaxLength(150)
                .HasColumnName("selection_mode");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.Title).HasMaxLength(50);
            entity.Property(e => e.VariantNoOfSelection).HasColumnName("variant_no_of_selection");
        });

        modelBuilder.Entity<VariantValue>(entity =>
        {
            entity.HasKey(e => e.VariantValueId).HasName("PK__VariantV__9DEECB9AD36DA4B4");

            entity.ToTable("VariantValue");

            entity.Property(e => e.VariantValueId).HasColumnName("VariantValueID");
            entity.Property(e => e.ProductId).HasColumnName("Product_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.Value).HasMaxLength(500);
            entity.Property(e => e.VariantImg).HasColumnName("variant_img");
            entity.Property(e => e.VariantPrice).HasColumnName("variant_price");
            entity.Property(e => e.VariantStock).HasColumnName("variant_stock");
            entity.Property(e => e.VariantTypeId).HasColumnName("VariantTypeID");

            entity.HasOne(d => d.VariantType).WithMany(p => p.VariantValues)
                .HasForeignKey(d => d.VariantTypeId)
                .HasConstraintName("FK_VariantValue_VariantType");
        });

        modelBuilder.Entity<WebType>(entity =>
        {
            entity.HasKey(e => e.WebId);

            entity.ToTable("Web_Type");

            entity.Property(e => e.WebId).HasColumnName("web_id");
            entity.Property(e => e.AddedBy)
                .HasMaxLength(250)
                .HasColumnName("added_by");
            entity.Property(e => e.TransactionDt)
                .HasColumnType("datetime")
                .HasColumnName("transaction_dt");
            entity.Property(e => e.WebTypeDesc)
                .HasMaxLength(500)
                .HasColumnName("web_type_desc");
            entity.Property(e => e.WebTypeImg)
                .HasMaxLength(500)
                .HasColumnName("web_type_img");
            entity.Property(e => e.WebTypeName)
                .HasMaxLength(250)
                .HasColumnName("web_type_name");
        });

        modelBuilder.Entity<WeightDetail>(entity =>
        {
            entity.HasKey(e => e.WeightId);

            entity.ToTable("Weight_Detail");

            entity.Property(e => e.WeightId).HasColumnName("weight_id");
            entity.Property(e => e.EColumn)
                .HasMaxLength(150)
                .HasColumnName("e_column");
            entity.Property(e => e.WeightPrice).HasColumnName("weight_price");
            entity.Property(e => e.WeightRange).HasColumnName("weight_range");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
