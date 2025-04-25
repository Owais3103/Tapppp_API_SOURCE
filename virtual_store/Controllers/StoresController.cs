using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Security.Cryptography;
using virtual_store.Models;
using virtual_store.settingclasses;
using VariantType = virtual_store.Models.VariantType;

namespace virtual_store.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly VirtualStoreContext db;
        private readonly IConfiguration _configuration;
        private readonly OrderService _orderService;
        private readonly IHubContext<OrderHub> _hubContext;

        public StoresController(VirtualStoreContext _db, IConfiguration configuration, OrderService orderService, IHubContext<OrderHub> hubContext)
        {
            db = _db;
            _configuration = configuration;
            _orderService = orderService;
            _hubContext = hubContext;
        }


        public static DateTime PST()
        {
            DateTime utcNow = DateTime.UtcNow;

            // Define the Pakistan Standard Time zone (use "Asia/Karachi" as an alternative ID)
            TimeZoneInfo pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Karachi");

            // Convert the UTC time to Pakistan Standard Time
            DateTime pakistanTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, pakistanTimeZone);

            // Assign it to your desired variable
            var tds = pakistanTime;
            return tds;
        }

        [HttpPost("Category")]
        [Authorize]
        public IActionResult fetch_category(int id)
        {
            var fetch_category_by_id = db.Categories.OrderByDescending(c => c.SequenceOrder).Where(m => m.StoreId == id && m.CategoryDesc == "Active").ToList();
            return Ok(new { categories = fetch_category_by_id });
        }
        [HttpPost("Category_add")]
        //[Authorize]
        public IActionResult AddCategory([FromBody] List<Category> categoryDto)
        {
            try
            {
               

                foreach (var item in categoryDto)
                {

                    var categories = db.Categories
                          .Where(c => c.StoreId == item.StoreId &&c.CategoryDesc=="Active")
                          .OrderBy(c => c.SequenceOrder)
                          .ToList();

                    // Step 1: Normalize sequence (1,2,3,4,5)
                    int sequence = 1;
                    foreach (var cat in categories)
                    {
                        cat.SequenceOrder = sequence;
                        sequence++;
                    }
                    db.SaveChanges();

                    // Step 2: Shift all sequence +1 to make space for new category
                    db.Database.ExecuteSqlRaw("UPDATE Categories SET sequence_order = sequence_order + 1 WHERE store_id = {0}", item.StoreId);

                    // Step 3: Add new category at SequenceOrder = 1
                    var category = new Category
                    {
                        CategoryName = item.CategoryName,
                        CategoryDesc = "Active",
                        SequenceOrder = 1,
                        StoreId = item.StoreId
                    };

                    db.Categories.Add(category);
                    db.SaveChanges();
                }

                return Ok(new { success = true, message = "Category added successfully." });
            }
            catch (Exception e)
            {
                return BadRequest(new { success = false, error = e.Message });
            }
        }



        //      [HttpPost("get_category_by_id")]
        //    [Authorize]
        //      public IActionResult get_category_by_id(int id) { 

        //          var category = db.Categories.FirstOrDefault(m => m.CategoryId == id);
        //          if (category != null)
        //          {
        //return Ok(category);
        //          }
        //          else
        //          {
        //              return BadRequest(new {error= "Not Found"});
        //          }


        //      }
        [HttpPost("update_category_by_id")]
        [Authorize]
        public IActionResult update_category_by_id(int id, string catname)
        {
            try
            {
                var getcategory = db.Categories.Where(m => m.CategoryId == id).FirstOrDefault();

                if (getcategory != null)
                {
                    getcategory.CategoryName = catname;
                    db.Categories.Update(getcategory);
                    db.SaveChanges(); return Ok(new { sucess = "Category Updated" });
                }
                return Ok(new { sucess = "Not Found" });

            }
            catch (Exception er)
            {
                return BadRequest(new { error = "Error While Update Product'" + er + "'" });
            }
        }

        //SHIPPING SETTING START/////////////////////////////////////////

        [HttpPost("Method")]
        [Authorize]
        public IActionResult Method()
        {
            Dictionary<string, List<object>> dsdsd = new Dictionary<string, List<object>>();
            var query = (from zone in db.PayDetails
                         join ship in db.PaymentMethods
                         on zone.Paymentid equals ship.Paymentid
                         select new
                         {
                             withcity = zone.WithinCityPrice,
                             outcity = zone.OutOfCityPrice,
                             MethodName = ship.PaymentMethodName
                         })
                     .Distinct()
                     .ToList();

            // Group zones under each MethodName
            foreach (var item in query)
            {
                if (!dsdsd.ContainsKey(item.MethodName))
                {
                    dsdsd[item.MethodName] = new List<object>();
                }

                dsdsd[item.MethodName].Add(new
                {
                    withcity = item.withcity,
                    outcity = item.outcity,
                });
            }

            return Ok(dsdsd);

        }


        [HttpPost("fetch_price")]

        public IActionResult fetch_price(int store_id, string city, string method, int kg)
        {

            var price_by_city = 0;
            var kgprice = 0;
            if (kg > 1)
            {
                kg = kg - 1;
                kgprice = 100 * kg;
            }
            var getstore = db.Stores.Where(m => m.StoreId == store_id).AsNoTracking().FirstOrDefault();
            getstore.CommisionShipmentPrice = getstore.CommisionShipmentPrice ?? 0;
            if (getstore != null)
            {

                if (getstore.StoreCity == null)
                {
                    return Ok(new { store = "City in Store not Found" });
                }

                var getmethod = db.PaymentMethods.Where(m => m.PaymentMethodName == method).FirstOrDefault();
                var get_postex_cities = db.Cities.Where(m => m.CityName == city).FirstOrDefault();

                if (get_postex_cities == null)
                {


                    var getprice = db.PayDetails.Where(m => m.Paymentid == getmethod.Paymentid).FirstOrDefault();
                    if (getstore.StoreCity == city)
                    {
                        price_by_city = Convert.ToInt32(getprice.WithinCityPrice + getstore.CommisionShipmentPrice + 15);
                    }
                    else
                    {
                        price_by_city = Convert.ToInt32(getprice.OutOfCityPrice + getstore.CommisionShipmentPrice);
                    }


                }
                else
                {
                    var getprice = db.PayDetails.Where(m => m.Paymentid == getmethod.Paymentid).FirstOrDefault();
                    if (getstore.StoreCity == city)
                    {
                        price_by_city = Convert.ToInt32(getprice.WithinCityPrice + getstore.CommisionShipmentPrice);
                    }
                    else
                    {
                        price_by_city = Convert.ToInt32(getprice.OutOfCityPrice + getstore.CommisionShipmentPrice);
                    }

                }

                price_by_city = price_by_city + kgprice;

                return Ok(new { price = price_by_city });
            }
            else
            {
                return Ok(new { store = "Not Found" });
            }
        }


        [HttpPost("fetch_retail_price")]

        public IActionResult fetch_retail_price(int store_id, int aria_id)
        {


            var get_price = db.AriaDetails.Where(s => s.StoreId == store_id && s.AriaId == aria_id).FirstOrDefault();

            return Ok(new { price = get_price.Price });

        }



        [HttpPost("fetch_arias")]

        public IActionResult fetch_arias(int store_id)
        {


            var get_arias = db.AriaDetails.Where(s => s.StoreId == store_id).AsNoTracking().ToList();

            return Ok(new { arias_list = get_arias });

        }


        [HttpPost("fetch_pickup")]

        public IActionResult fetch_pickup(int store_id)
        {


            var fetch_pickup = db.PickupDetails.Where(s => s.StoreId == store_id).AsNoTracking().ToList();

            return Ok(new { arias_list = fetch_pickup });

        }



        [HttpPost("update_status_by_mid")]
        [Authorize]
        public IActionResult update_status_by_mid(int store_id, int mid, string new_status)
        {
            try
            {
                var get_order_master = db.OrderMasters.Find(mid);

                get_order_master.OrderStatus = new_status;
                get_order_master.LastUpdateDate = PST();
                get_order_master.IsSeen = 2;
                db.OrderMasters.Update(get_order_master);
                db.SaveChanges();

                return Ok(new { success = "Status Updated Successfully..." });
            }
            catch (Exception e)
            {
                return BadRequest(new { success = "Error White Status Updating ..." + e });
            }
        }

        //SHIPPING SETTING END /////////////////////////////////////////

        [HttpPost("delete_product")]
        [Authorize]
        public IActionResult delete_product(int id)
        {

            var product = db.Products.FirstOrDefault(m => m.ProductId == id);
            if (product != null)
            {
                product.EColumn = "InActive";
                db.Products.Update(product);
                db.SaveChanges();
                return Ok(new { success = "Product Delete" });
            }
            else
            {
                return BadRequest(new { error = "Not Found" });
            }
        }

        [HttpPost("delete_category")]
        // [Authorize]
        public IActionResult DeleteCategory(int id)
        {
            var category = db.Categories.FirstOrDefault(c => c.CategoryId == id);
            if (category == null)
            {
                return Ok(new { success = "Category Not Found" });
            }
            category.CategoryDesc = "InActive";
            var products = db.Products.Where(p => p.CategoryId == id).ToList();

            if (products.Any())
            {
                products.ForEach(p => p.EColumn = "InActive");
                db.Products.UpdateRange(products);
            }

            db.Categories.Update(category);
            db.SaveChanges();

            return Ok(new { success = "Category Deleted Successfully" });
        }

        [HttpPost("Product")]
        // [Authorize]
        public IActionResult fetch_product_for_addson(int id)
        {
            var fetch_product_by_id = db.Products.OrderBy(x => x.SequenceOrder).Where(m => m.StoreId == id && m.EColumn == "Active" && m.Isactive == "Active").ToList();
            return Ok(new { products = fetch_product_by_id });
        }

        [HttpPost("weight_range")]
        public async Task<IActionResult> weight_range()
        {
            //       await _orderService.AddNewOrder(); // Await added
            var get_weight_range = db.WeightDetails.ToList();
            return Ok(get_weight_range);
        }

        [HttpPost("AddProduct")]
        //  [Authorize]
        public async Task<IActionResult> Add_Product([FromBody] Product_Add product)
        {
            try
            {
                // Validate input
                if (product == null || string.IsNullOrWhiteSpace(product.name))
                {
                    return BadRequest(new { success = false, message = "Product details are invalid or missing." });
                }
                // Handle category
                int categoryId = categoryId = product.category_id;

                // Step 2: Handle product image upload
                var productImageUrl = "";
                if (!string.IsNullOrEmpty(product.image))
                {
                    string base64Image = product.image.Split(',')[1]; // Remove the "data:image/png;base64," prefix
                    byte[] imageBytes = Convert.FromBase64String(base64Image);
                    var stream = new MemoryStream(imageBytes);

                    // Create an IFormFile from the base64 string  // Create a unique file name
                    var fileName = Guid.NewGuid().ToString() + "_product_image.png";

                    // Create an IFormFile from the base64 string
                    var file = new FormFile(stream, 0, imageBytes.Length, "file", fileName)
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = "image/png", // Set the appropriate content type
                        ContentDisposition = $"form-data; name=\"file\"; filename=\"{fileName}\""
                    };
                    // Upload the file to S3 or save it to a folder
                    productImageUrl = await UploadToS3(file);  // Example method to upload to S3
                }


                // Step 1: Normalize existing product sequence for this store & category
                var existingProducts = db.Products
                    .Where(p => p.StoreId == product.store_id && p.CategoryId == categoryId && p.EColumn=="Active")
                    .OrderBy(p => p.SequenceOrder)
                    .ToList();

                int productSequence = 1;
                foreach (var p in existingProducts)
                {
                    p.SequenceOrder = productSequence++;
                }
                await db.SaveChangesAsync();

                // Step 2: Shift all sequence +1 to make room at top
                await db.Database.ExecuteSqlRawAsync("UPDATE Products SET sequence_order = sequence_order + 1 WHERE Store_id = {0} AND category_id = {1}", product.store_id, categoryId);


                // Save the product entity to the database
                var productEntity = new Product
                {
                    ProductName = product.name,
                    ProductDesc = product.description,
                    ProductPrice = Convert.ToInt32(product.price),
                    SalesTax = Convert.ToInt32(product.sales_tax),
                    Discount = Convert.ToInt32(product.discount),
                    ProductImg = productImageUrl,
                    CategoryId = categoryId,
                    Isactive = "Active",
                    TransactionDt = PST(),
                    IsLimitedSelection = product.IsLimitedSelection,
                    NumberOfSelection = product.NumberOfSelection,
                    WeightId = product.WeightId,
                    EColumn = "Active",
                    SequenceOrder = 1,
                    StoreId = product.store_id
                };
                db.Products.Add(productEntity);
                await db.SaveChangesAsync();
                var getlastproduct = db.Products.Where(m => m.CategoryId == productEntity.CategoryId && m.StoreId == productEntity.StoreId && productEntity.ProductName == productEntity.ProductName).ToList().Last()
;
                if (product.MultipleImges != null)
                {
                    foreach (var item in product.MultipleImges)
                    {
                        var MultiproductImageUrl = "";
                        if (!string.IsNullOrEmpty(item.ImageUrl))
                        {
                            string base64Image = item.ImageUrl.Split(',')[1]; // Remove the "data:image/png;base64," prefix
                            byte[] imageBytes = Convert.FromBase64String(base64Image);
                            var stream = new MemoryStream(imageBytes);

                            // Create an IFormFile from the base64 string  // Create a unique file name
                            var fileName = Guid.NewGuid().ToString() + "_product_multi_image.png";

                            // Create an IFormFile from the base64 string
                            var file = new FormFile(stream, 0, imageBytes.Length, "file", fileName)
                            {
                                Headers = new HeaderDictionary(),
                                ContentType = "image/png", // Set the appropriate content type
                                ContentDisposition = $"form-data; name=\"file\"; filename=\"{fileName}\""
                            };
                            // Upload the file to S3 or save it to a folder
                            MultiproductImageUrl = await UploadToS3(file);  // Example method to upload to S3
                        }
                        ProductsImage productsImage = new ProductsImage();
                        productsImage.ImageUrl = MultiproductImageUrl;
                        productsImage.ProductId = getlastproduct.ProductId;
                        productsImage.StoreId = product.store_id;
                        productsImage.IsActive = 1;
                        db.ProductsImages.Add(productsImage);
                        db.SaveChanges();

                    }
                }
                // Handle Addons
                if (product.Addons != null)
                {
                    foreach (var addonGroup in product.Addons)
                    {
                        var variantType = new VariantType
                        {
                            StoreId = product.store_id,
                            ProductId = getlastproduct.ProductId,
                            SelectionMode = "Multiple",
                            Title = addonGroup.Key


                        };
                        db.VariantTypes.Add(variantType);
                        await db.SaveChangesAsync();

                        foreach (var addon in addonGroup.Value)
                        {
                            var variantValue = new VariantValue
                            {
                                VariantTypeId = variantType.VariantTypeId,
                                Value = addon.Name,
                                ProductId = addon.Id,
                                StoreId = product.store_id
                            };
                            db.VariantValues.Add(variantValue);
                        }
                    }
                }
                // Handle Dropdowns
                if (product.Dropdowns != null)
                {
                    foreach (var dropdownGroup in product.Dropdowns)
                    {
                        var dr = dropdownGroup.Value.FirstOrDefault();

                        var variantType = new VariantType
                        {
                            StoreId = product.store_id,
                            ProductId = getlastproduct.ProductId,
                            SelectionMode = "Single",
                            Title = dropdownGroup.Key,
                            IsVariantLimitSelection = dr.IsVariantLimitSelection,
                            VariantNoOfSelection = dr.VariantNoOfSelection

                        };
                        db.VariantTypes.Add(variantType);
                        await db.SaveChangesAsync();

                        foreach (var dropdown in dropdownGroup.Value)
                        {
                            var dropdownImageUrl = "";

                            if (!string.IsNullOrEmpty(dropdown.Image))
                            {
                                string base64Image = dropdown.Image.Split(',')[1]; // Remove the "data:image/png;base64," prefix
                                byte[] imageBytes = Convert.FromBase64String(base64Image);
                                var stream = new MemoryStream(imageBytes);

                                // Create an IFormFile from the base64 string  // Create a unique file name
                                var fileName = Guid.NewGuid().ToString() + "_product_image.png";

                                // Create an IFormFile from the base64 string
                                var file = new FormFile(stream, 0, imageBytes.Length, "file", fileName)
                                {
                                    Headers = new HeaderDictionary(),
                                    ContentType = "image/png", // Set the appropriate content type
                                    ContentDisposition = $"form-data; name=\"file\"; filename=\"{fileName}\""
                                };
                                // Upload the file to S3 or save it to a folder
                                dropdownImageUrl = await UploadToS3(file);  // Example method to upload to S3
                            }

                            var variantValue = new VariantValue
                            {
                                VariantTypeId = variantType.VariantTypeId,
                                Value = dropdown.Name,
                                VariantImg = dropdownImageUrl,
                                VariantPrice = dropdown.Price,
                                StoreId = product.store_id
                            };
                            db.VariantValues.Add(variantValue);
                        }
                    }
                }
                await db.SaveChangesAsync();
                return Ok(new { success = true, message = "Product added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
        private async Task<string> UploadToS3(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;
            try
            {
                var accessKey = _configuration["AWS:AccessKey"];
                var secretKey = _configuration["AWS:SecretKey"];
                var bucketName = _configuration["AWS:BucketName"];
                var region = _configuration["AWS:Region"];

                var s3Client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.GetBySystemName(region));
                var fileName = Guid.NewGuid() + "_" + file.FileName;

                using (var stream = file.OpenReadStream())
                {
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = stream,
                        Key = fileName,
                        BucketName = bucketName,
                        ContentType = file.ContentType
                    };
                    var transferUtility = new TransferUtility(s3Client);
                    await transferUtility.UploadAsync(uploadRequest);
                }

                return $"https://{bucketName}.s3.{region}.amazonaws.com/{fileName}";
            }
            catch
            {
                return null; // Log the error for debugging
            }
        }

        [HttpPost("checkstore")]
        public IActionResult checkstore(string storelink)
        {

            var checkaccess = db.AccessDetails.Where(m => m.AccessLink == storelink).FirstOrDefault();
            var getstoreidbystorelink = db.Stores.Where(m => m.StoreId == checkaccess.StoreId).FirstOrDefault();
            if (getstoreidbystorelink != null)
            {
                return Ok(getstoreidbystorelink);
            }
            else
            {
                return BadRequest(new { notfound = "notfound" });
            }
        }

        [HttpPost("fetch_product")]
        public IActionResult Feth_product(int storeid)
        {


            var getstore = db.Stores.Find(storeid);

            if (getstore.BusinessType == "ecommerce")
            {


                List<Dictionary<int, object>> ints = new List<Dictionary<int, object>>();
                Product_Add ap = new Product_Add();
                var get_product = db.Products.Where(m => m.StoreId == storeid && m.Isactive == "Active").ToList();

                // Fetch VariantTypes and transform them into the intended Dropdownss structure
                var variantTypes = db.VariantTypes
                    .Select(vt => new Dropdown_Fetch
                    {
                        VariantTypeId = vt.VariantTypeId,
                        ProductId = Convert.ToInt32(vt.ProductId),
                        Title = vt.Title,
                        IsVariantLimitSelection = vt.IsVariantLimitSelection,
                        VariantNoOfSelection = vt.VariantNoOfSelection,
                        Options = vt.VariantValues
                            .Select(vv => new DropdownOption
                            {
                                VariantValueId = vv.VariantValueId,
                                Value = vv.Value,
                                VariantImg = vv.VariantImg,

                                VariantPrice = vv.VariantPrice
                            })
                            .ToList()
                    })
                    .ToList();

                // Now, we'll map the product list, including matching the product ID with the variant types

                var productAddList = get_product.Select(product => new Product_AddWithDropdown
                {
                    ProductId = product?.ProductId ?? 0,
                    Name = product?.ProductName ?? "Unknown Product",
                    Description = product?.ProductDesc ?? "No Description",
                    Price = product?.ProductPrice?.ToString() ?? "0",
                    SalesTax = product?.SalesTax?.ToString() ?? "0",
                    so = product?.SequenceOrder ?? 0,
                    Discount = product?.Discount?.ToString() ?? "0",
                    EColumn = product.EColumn,
                    WeightId = product.WeightId,
                   
                    MultipleImges = db.ProductsImages.Where(p => p.ProductId == product.ProductId).ToList(),
                    IsActive = "Active",
                    IsLimitedSelection = product.IsLimitedSelection,
                    NumberOfSelection = product.NumberOfSelection,


                    WeightPrice = db.WeightDetails.Where(m => m.WeightId == product.WeightId).FirstOrDefault().WeightPrice,
                    WeightRange = db.WeightDetails.Where(m => m.WeightId == product.WeightId).FirstOrDefault().WeightRange,
                    Image = product?.ProductImg ?? "No Image",
                    CategoryId = product?.CategoryId ?? 0,
                    Category = db.Categories.Where(m => m.CategoryId == product.CategoryId).Select(m => m.CategoryName).FirstOrDefault(),
                    StoreId = product?.StoreId ?? 0,

                    // Add the dropdown for variant types (these are the available options for the product)
                    VariantTypeDropdowns = db.VariantTypes.Where(m => m.SelectionMode == "Single")
                    .Select(vt => new Dropdown_Fetch
                    {
                        VariantTypeId = vt.VariantTypeId,
                        ProductId = Convert.ToInt32(vt.ProductId),
                        Title = vt.Title,
                        IsVariantLimitSelection = vt.IsVariantLimitSelection,
                        VariantNoOfSelection = vt.VariantNoOfSelection,
                        Options = vt.VariantValues
                            .Select(vv => new DropdownOption
                            {
                                VariantValueId = vv.VariantValueId,
                                Value = vv.Value,
                                VariantImg = vv.VariantImg,

                                VariantPrice = vv.VariantPrice
                            })
                            .ToList()
                    }).Where(m => m.ProductId == product.ProductId)
                    .ToList(),

                    // Default empty addons, you can update this logic to select specific addons if needed
                    Addons = db.VariantTypes.Where(m => m.SelectionMode == "Multiple")
                    .Select(vt => new Addon_Fetch
                    {
                        VariantTypeId = vt.VariantTypeId,
                        ProductId = Convert.ToInt32(vt.ProductId),

                        Title = vt.Title,
                        Products = db.Products
        .Where(m => vt.VariantValues.Select(vv => vv.ProductId).Contains(m.ProductId) && m.EColumn == "Active" && m.Isactive == "Active")
        .Select(p => new Product_Addon
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            ProductDesc = p.ProductDesc,
            ProductPrice = p.ProductPrice,
            SalesTax = p.SalesTax,
            Discount = p.Discount,
            EColumn = p.EColumn,
            isactive = "Active",
            ProductImg = p.ProductImg,
            CategoryId = p.CategoryId.HasValue ? p.CategoryId.Value : 0,
            StoreId = p.StoreId.HasValue ? p.StoreId.Value : 0,
            WeightId = p.WeightId,
            WeightPrice = db.WeightDetails.Where(w => w.WeightId == p.WeightId).FirstOrDefault().WeightPrice, // Add logic to fetch weight price if needed
            WeightRange = db.WeightDetails.Where(w => w.WeightId == p.WeightId).FirstOrDefault().WeightRange, // Add logic to fetch weight range if needed
        })
        .ToList(),

                    }).Where(m => m.ProductId == product.ProductId)
                    .ToList(),
                }).OrderByDescending(c => c.so).Where(m => m.EColumn == "Active" && m.IsActive == "Active").ToList();

                var grr = productAddList
             .GroupBy(g => g.CategoryId)
             .OrderBy(g => db.Categories
                 .Where(c => c.CategoryId == g.Key)
                 .Select(c => c.SequenceOrder)
                 .FirstOrDefault()) // Order groups by SequenceOrder
             .ToDictionary(
                 g => g.Key, // Category ID as Key
                 g => g.ToList() // Convert each group into a list of products
             ).ToList();



                return Ok(grr);





            }
            else
            {




                List<Dictionary<int, object>> ints = new List<Dictionary<int, object>>();
                Product_Add ap = new Product_Add();
                var get_product = db.Products.Where(m => m.StoreId == storeid && m.Isactive == "Active").ToList();

                // Fetch VariantTypes and transform them into the intended Dropdownss structure
                var variantTypes = db.VariantTypes
                    .Select(vt => new Dropdown_Fetch
                    {
                        VariantTypeId = vt.VariantTypeId,
                        ProductId = Convert.ToInt32(vt.ProductId),
                        Title = vt.Title,
                        IsVariantLimitSelection = vt.IsVariantLimitSelection,
                        VariantNoOfSelection = vt.VariantNoOfSelection,
                        Options = vt.VariantValues
                            .Select(vv => new DropdownOption
                            {
                                VariantValueId = vv.VariantValueId,
                                Value = vv.Value,
                                VariantImg = vv.VariantImg,

                                VariantPrice = vv.VariantPrice
                            })
                            .ToList()
                    })
                    .ToList();

                // Now, we'll map the product list, including matching the product ID with the variant types

                var productAddList = get_product.Select(product => new Product_AddWithDropdown
                {
                    ProductId = product?.ProductId ?? 0,
                    Name = product?.ProductName ?? "Unknown Product",
                    Description = product?.ProductDesc ?? "No Description",
                    Price = product?.ProductPrice?.ToString() ?? "0",
                    SalesTax = product?.SalesTax?.ToString() ?? "0",
                    Discount = product?.Discount?.ToString() ?? "0",
                    EColumn = product.EColumn,
                    //    WeightId = product.WeightId,
                    MultipleImges = db.ProductsImages.Where(p => p.ProductId == product.ProductId).ToList(),
                    IsActive = "Active",
                    so = product?.SequenceOrder ?? 0,
                    IsLimitedSelection = product.IsLimitedSelection,
                    NumberOfSelection = product.NumberOfSelection,
                    //   WeightPrice = db.WeightDetails.Where(m => m.WeightId == product.WeightId).FirstOrDefault().WeightPrice,
                    // WeightRange = db.WeightDetails.Where(m => m.WeightId == product.WeightId).FirstOrDefault().WeightRange,
                    Image = product?.ProductImg ?? "No Image",
                    CategoryId = product?.CategoryId ?? 0,
                    Category = db.Categories.Where(m => m.CategoryId == product.CategoryId).Select(m => m.CategoryName).FirstOrDefault(),
                    StoreId = product?.StoreId ?? 0,

                    // Add the dropdown for variant types (these are the available options for the product)
                    VariantTypeDropdowns = db.VariantTypes.Where(m => m.SelectionMode == "Single")
                    .Select(vt => new Dropdown_Fetch
                    {
                        VariantTypeId = vt.VariantTypeId,
                        ProductId = Convert.ToInt32(vt.ProductId),
                        Title = vt.Title,
                        IsVariantLimitSelection = vt.IsVariantLimitSelection,
                        VariantNoOfSelection = vt.VariantNoOfSelection,
                        Options = vt.VariantValues
                            .Select(vv => new DropdownOption
                            {
                                VariantValueId = vv.VariantValueId,
                                Value = vv.Value,
                                VariantImg = vv.VariantImg,

                                VariantPrice = vv.VariantPrice
                            })
                            .ToList()
                    }).Where(m => m.ProductId == product.ProductId)
                    .ToList(),

                    // Default empty addons, you can update this logic to select specific addons if needed
                    Addons = db.VariantTypes.Where(m => m.SelectionMode == "Multiple")
                    .Select(vt => new Addon_Fetch
                    {
                        VariantTypeId = vt.VariantTypeId,
                        ProductId = Convert.ToInt32(vt.ProductId),

                        Title = vt.Title,
                        Products = db.Products
        .Where(m => vt.VariantValues.Select(vv => vv.ProductId).Contains(m.ProductId) && m.EColumn == "Active" && m.Isactive == "Active")
        .Select(p => new Product_Addon
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            ProductDesc = p.ProductDesc,
            ProductPrice = p.ProductPrice,
            SalesTax = p.SalesTax,
            Discount = p.Discount,
            EColumn = p.EColumn,
            isactive = "Active",
            ProductImg = p.ProductImg,
            CategoryId = p.CategoryId.HasValue ? p.CategoryId.Value : 0,
            StoreId = p.StoreId.HasValue ? p.StoreId.Value : 0,
            //  WeightId = p.WeightId,
            //  WeightPrice = db.WeightDetails.Where(w => w.WeightId == p.WeightId).FirstOrDefault().WeightPrice, // Add logic to fetch weight price if needed
            //  WeightRange = db.WeightDetails.Where(w => w.WeightId == p.WeightId).FirstOrDefault().WeightRange, // Add logic to fetch weight range if needed
        })
        .ToList(),

                    }).Where(m => m.ProductId == product.ProductId)
                    .ToList(),
                }).OrderByDescending(c => c.so).Where(m => m.EColumn == "Active" && m.IsActive == "Active").ToList();

                var grr = productAddList
                .GroupBy(g => g.CategoryId)
                .OrderBy(g => db.Categories
                    .Where(c => c.CategoryId == g.Key)
                    .Select(c => c.SequenceOrder)
                    .FirstOrDefault()) // Order groups by SequenceOrder
                .ToDictionary(
                    g => g.Key, // Category ID as Key
                    g => g.ToList() // Convert each group into a list of products
                ).ToList();
                //ap = get_product;
                return Ok(grr);

            }


        }





        [HttpPost("get_categories")]
        public async Task<IActionResult> get_categories(int storeid)
        {
            //   await _hubContext.Clients.Group(storeid.ToString()).SendAsync("ReceiveOrderNotification", "Dear  finduser.UserName  ew Order Placed for your store! Order NO orderMaster.OrderId");


            var get_category = db.Categories.OrderByDescending(c => c.SequenceOrder).Where(m => m.StoreId == storeid && m.CategoryDesc == "Active").ToList();
            return Ok(get_category);
        }





        [HttpPost("checkout")]
        public async Task<IActionResult> Ordered(ApiDataModel apiDataModel)
        {
            try
            {
                var getcustomer = db.Customers.Where(m => m.CustomerContact == apiDataModel.CustomerDetails.Contact && m.StoreId == apiDataModel.storeId).FirstOrDefault();
                Customer customer = new Customer();
                var getlastcustomer = 0;
                if (getcustomer != null)
                {
                    getcustomer.CustomerAddress = getcustomer.CustomerAddress;
                    getcustomer.CustomerName = getcustomer.CustomerName;
                    getcustomer.CustomerContact = apiDataModel.CustomerDetails.Contact;
                    getcustomer.City = apiDataModel.CustomerDetails.City;
                    getcustomer.CustomerAria = apiDataModel.CustomerDetails.customer_aria;
                    getcustomer.CityId = apiDataModel.CustomerDetails.city_id;
                    getcustomer.CountryId = apiDataModel.CustomerDetails.country_id;
                    getcustomer.ProvinceId = apiDataModel.CustomerDetails.province_id;
                    getcustomer.TransactionDt = PST();
                    getcustomer.StoreId = apiDataModel.storeId;
                    db.Customers.Update(getcustomer);
                    db.SaveChanges();
                    customer = getcustomer;
                    getlastcustomer = getcustomer.CustomerId;
                }
                else
                {
                    customer.CustomerAddress = apiDataModel.CustomerDetails.Address;
                    customer.CustomerName = apiDataModel.CustomerDetails.FirstName;
                    customer.CustomerContact = apiDataModel.CustomerDetails.Contact;
                    customer.City = apiDataModel.CustomerDetails.City;
                    customer.CustomerAria = apiDataModel.CustomerDetails.customer_aria;
                    customer.CityId = apiDataModel.CustomerDetails.city_id;
                    customer.CountryId = apiDataModel.CustomerDetails.country_id;
                    customer.ProvinceId = apiDataModel.CustomerDetails.province_id;
                    customer.TransactionDt = PST();
                    customer.StoreId = apiDataModel.storeId;
                    db.Customers.Add(customer);
                    db.SaveChanges();
                    getlastcustomer = db.Customers.Where(m => m.StoreId == customer.StoreId && m.CustomerAddress == customer.CustomerAddress && m.CustomerContact == customer.CustomerContact).AsNoTracking().ToList().Last().CustomerId;

                }
                OrderMaster orderMaster = new OrderMaster();
                var coun = db.OrderSerials.Count();
                if (coun > 0)
                {
                    var getlast = db.OrderSerials.ToList().Last();
                    string spil = getlast.SeriesCode.Substring(0, 1);
                    int spil2 = Convert.ToInt16(getlast.SeriesCode.Substring(1));
                    int addone = spil2 + 1;
                    var ser = new OrderSerial
                    {
                        SeriesCode = spil + addone,
                        Storeid = customer.StoreId,

                    };
                    db.OrderSerials.Add(ser);
                    orderMaster.OrderId = ser.SeriesCode;
                }
                else
                {
                    var ser = new OrderSerial
                    {
                        SeriesCode = "#1001",
                        Storeid = customer.StoreId,
                    };
                    db.OrderSerials.Add(ser);
                    orderMaster.OrderId = "#1001";
                }
                var getpd = db.PaymentMethods.Where(m => m.PaymentMethodName == apiDataModel.PaymentMethodDetails).FirstOrDefault();

                orderMaster.Address = customer.CustomerAddress;
                orderMaster.StoreId = customer.StoreId;
                orderMaster.CustomerId = getlastcustomer;
                orderMaster.City = apiDataModel.CustomerDetails.City;
                orderMaster.TotalPrice = Convert.ToInt32(apiDataModel.TotalPayment);
                orderMaster.TotalWeight = apiDataModel.totalWeight;
                orderMaster.OrderStatus = "Pending";
                orderMaster.ShippingPrice = apiDataModel.ShippingCost;
                orderMaster.OrderType = apiDataModel.order_type;
                orderMaster.Discount = Convert.ToInt32(apiDataModel.Discount);
                orderMaster.IsSeen = 1;
                orderMaster.SalesTax =Convert.ToDouble( apiDataModel.sales_tax);
                orderMaster.CustomerInstruction = apiDataModel.customer_instructions;
                orderMaster.Quantity = apiDataModel.CartItems.Count();
                orderMaster.ItemPrice = apiDataModel.item_price;
                orderMaster.CityId = apiDataModel.CustomerDetails.city_id;
                orderMaster.ProvinceId = apiDataModel.CustomerDetails.province_id;
                orderMaster.CountryId = apiDataModel.CustomerDetails.country_id;
                //    orderMaster.ShippingMethods = apiDataModel.ShippingMethodDetails.ShippingMethodId;
                orderMaster.PaymentMethod = getpd.Paymentid;
                orderMaster.PaymentMethodName = getpd.PaymentMethodName;
                orderMaster.TransactionDt = PST();
                db.OrderMasters.Add(orderMaster);
                db.SaveChanges();
                var getlastordermaster = db.OrderMasters.Where(m => m.StoreId == orderMaster.StoreId && m.CustomerId == orderMaster.CustomerId && m.OrderId == orderMaster.OrderId).AsNoTracking().ToList().Last().Mid;

                foreach (var item in apiDataModel.CartItems)
                {
                    OrderDetail orderDetail = new OrderDetail();
                    orderDetail.ProductId = item.ProductId;
                    orderDetail.ProductItem = item.Quantity;
                    orderDetail.ProductImage = item.Image;
                    orderDetail.Mid = getlastordermaster;
                    orderDetail.Price = Convert.ToInt32(item.Price);
                    orderDetail.StoreId = orderMaster.StoreId;
                    orderDetail.TransactionDt = PST();
                    orderDetail.WeightId = item.weight_id;
                    orderDetail.WeightPrice = item.weight_price;
                    orderDetail.WeightRange = item.weight_range.ToString();
                    db.OrderDetails.Add(orderDetail);
                    db.SaveChanges();
                    var getlastorderdetailid = db.OrderDetails.Where(m => m.Mid == orderDetail.Mid && m.StoreId == orderDetail.StoreId).AsNoTracking().ToList().Last().Did;
                    if (item.SelectedVariants != null)
                    {
                        var orderDetailSubDetails = new List<OrderDetailSubDetail>();

                        foreach (var variants in item.SelectedVariants)
                        {
                            if (variants.Value != null && variants.Value.Length > 0) // Ensure the array is not empty
                            {
                                foreach (var value in variants.Value) // Loop through each variant value
                                {
                                    OrderDetailSubDetail orderDetailSubDetail = new OrderDetailSubDetail
                                    {
                                        AttributeValue = variants.Key,   // Key (e.g., 1201, 1202, etc.)
                                        AttributeName = value,          // Each selected variant
                                        Did = getlastorderdetailid,
                                        StoreId = orderDetail.StoreId,
                                        CreatedBy = "Owais",
                                        CreatedAt = PST()
                                    };

                                    orderDetailSubDetails.Add(orderDetailSubDetail);
                                }
                            }
                        }

                        var finduser = db.Users.Where(c => c.StoreId == orderDetail.StoreId).FirstOrDefault();

                        await _hubContext.Clients.Group(orderMaster.StoreId.ToString()).SendAsync("ReceiveOrderNotification", " Dear " + finduser.UserName + " New Order Placed for your store! Order NO " + orderMaster.OrderId + "");

                        db.OrderDetailSubDetails.AddRange(orderDetailSubDetails);
                        db.SaveChanges(); // Save all changes at once for efficiency
                                          // Serialize only necessary data

                    }

                }
                receipt receip = new receipt();
                receip.FirstName = apiDataModel.CustomerDetails.FirstName;
                receip.Address = apiDataModel.CustomerDetails.Address;
                receip.Contact = customer.CustomerContact;
                receip.City = orderMaster.City;
                receip.customer_aria = customer.CustomerAria;
                receip.ship_Price = apiDataModel.ShippingCost;
                receip.PaymentMethodName = getpd.PaymentMethodName;
                receip.Pay_Price = apiDataModel.TotalPayment;
                receip.item_price = orderMaster.ItemPrice;
                receip.customer_instruction = orderMaster.CustomerInstruction;
                receip.order_type = orderMaster.OrderType;
                receip.OrderId = orderMaster.OrderId;
                receip.total_price = orderMaster.TotalPrice.ToString();
                receip.order_datetime = orderMaster.TransactionDt.ToString();
                receip.discount =Convert.ToInt32( orderMaster.Discount);
                receip.sales_tax = orderMaster.SalesTax.ToString();
                return Ok(new { message = "done", Receipt = receip });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e });
            }

        }

        [HttpPost("customers")]

        // [Authorize]
        public IActionResult Customers(int storeid)
        {
            var customerList = db.Customers
                .Include(c => c.OrderMasters)
                .Select(v => new CustomerDTO
                {
                    CustomerName = v.CustomerName,
                    ProvinceId = v.OrderMasters.Count,
                    CityId = v.OrderMasters.Where(m => m.CustomerId == v.CustomerId).Select(s => s.TotalPrice).Sum(),
                    store_id = v.StoreId,

                    OrderMasters = db.OrderMasters.Where(b => b.CustomerId == v.CustomerId  ).ToList(),

                })
                .Where(s => s.store_id == storeid)
                .ToList();

            return Ok(customerList.Where(x=>x.OrderMasters.Count()>0).ToList());
        }

        [HttpPost("fetch_product_Admin")]
        public IActionResult Feth_product_Admin(int storeid)
        {


            var getstore = db.Stores.Find(storeid);

            if (getstore.BusinessType == "ecommerce")
            {



                List<Dictionary<int, object>> ints = new List<Dictionary<int, object>>();

                Product_Add ap = new Product_Add();
                var get_product = db.Products.Where(m => m.StoreId == storeid).ToList();

                // Fetch VariantTypes and transform them into the intended Dropdownss structure
                var variantTypes = db.VariantTypes
                    .Select(vt => new Dropdown_Fetch
                    {
                        VariantTypeId = vt.VariantTypeId,
                        ProductId = Convert.ToInt32(vt.ProductId),
                        Title = vt.Title,
                        IsVariantLimitSelection = vt.IsVariantLimitSelection,
                        VariantNoOfSelection = vt.VariantNoOfSelection,
                        Options = vt.VariantValues
                            .Select(vv => new DropdownOption
                            {
                                VariantValueId = vv.VariantValueId,
                                Value = vv.Value,
                                VariantImg = vv.VariantImg,

                                VariantPrice = vv.VariantPrice
                            })
                            .ToList()
                    })
                    .ToList();

                // Now, we'll map the product list, including matching the product ID with the variant types

                var productAddList = get_product.Select(product => new Product_AddWithDropdown
                {
                    ProductId = product?.ProductId ?? 0,
                    Name = product?.ProductName ?? "Unknown Product",
                    Description = product?.ProductDesc ?? "No Description",
                    Price = product?.ProductPrice?.ToString() ?? "0",
                    SalesTax = product?.SalesTax?.ToString() ?? "0",
                    Discount = product?.Discount?.ToString() ?? "0",
                    EColumn = product.EColumn,
                    IsLimitedSelection = product.IsLimitedSelection,
                    NumberOfSelection = product.NumberOfSelection,
                    WeightId = product.WeightId,
                    so = product?.SequenceOrder ?? 0,
                    MultipleImges = db.ProductsImages.Where(p => p.ProductId == product.ProductId).ToList(),
                    IsActive = product.Isactive,
                    WeightPrice = db.WeightDetails.Where(m => m.WeightId == product.WeightId).FirstOrDefault().WeightPrice,
                    WeightRange = db.WeightDetails.Where(m => m.WeightId == product.WeightId).FirstOrDefault().WeightRange,
                    Image = product?.ProductImg ?? "No Image",
                    CategoryId = product?.CategoryId ?? 0,
                    Category = db.Categories.Where(m => m.CategoryId == product.CategoryId).Select(m => m.CategoryName).FirstOrDefault(),
                    StoreId = product?.StoreId ?? 0,

                    // Add the dropdown for variant types (these are the available options for the product)
                    VariantTypeDropdowns = db.VariantTypes.Where(m => m.SelectionMode == "Single")
                    .Select(vt => new Dropdown_Fetch
                    {
                        VariantTypeId = vt.VariantTypeId,
                        ProductId = Convert.ToInt32(vt.ProductId),
                        Title = vt.Title,
                        IsVariantLimitSelection = vt.IsVariantLimitSelection,
                        VariantNoOfSelection = vt.VariantNoOfSelection,
                        Options = vt.VariantValues
                            .Select(vv => new DropdownOption
                            {
                                VariantValueId = vv.VariantValueId,
                                Value = vv.Value,
                                VariantImg = vv.VariantImg,

                                VariantPrice = vv.VariantPrice
                            })
                            .ToList()
                    }).Where(m => m.ProductId == product.ProductId)
                    .ToList(),

                    // Default empty addons, you can update this logic to select specific addons if needed


                    Addons = db.VariantTypes.Where(m => m.SelectionMode == "Multiple")
                    .Select(vt => new Addon_Fetch
                    {
                        VariantTypeId = vt.VariantTypeId,
                        ProductId = Convert.ToInt32(vt.ProductId),

                        Title = vt.Title,
                        Products = db.Products
        .Where(m => vt.VariantValues.Select(vv => vv.ProductId).Contains(m.ProductId) && m.EColumn == "Active")
        .Select(p => new Product_Addon
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            ProductDesc = p.ProductDesc,
            ProductPrice = p.ProductPrice,
            SalesTax = p.SalesTax,
            Discount = p.Discount,
            EColumn = p.EColumn,
            isactive = p.Isactive,
            ProductImg = p.ProductImg,
            CategoryId = p.CategoryId.HasValue ? p.CategoryId.Value : 0,
            StoreId = p.StoreId.HasValue ? p.StoreId.Value : 0,
            WeightId = p.WeightId,
            WeightPrice = db.WeightDetails.Where(w => w.WeightId == p.WeightId).FirstOrDefault().WeightPrice, // Add logic to fetch weight price if needed
            WeightRange = db.WeightDetails.Where(w => w.WeightId == p.WeightId).FirstOrDefault().WeightRange, // Add logic to fetch weight range if needed

        })
        .ToList(),

                    }).Where(m => m.ProductId == product.ProductId)
                    .ToList(),

                }).OrderByDescending(c=>c.so).Where(m => m.EColumn == "Active").ToList();

                var grr = productAddList
                       .GroupBy(g => g.CategoryId)
                       .OrderBy(g => db.Categories
                           .Where(c => c.CategoryId == g.Key)
                           .Select(c => c.SequenceOrder)
                           .FirstOrDefault()) // Order groups by SequenceOrder
                       .ToDictionary(
                           g => g.Key, // Category ID as Key
                           g => g.ToList() // Convert each group into a list of products
                       ).ToList();



                return Ok(grr);


            }
            else
            {


                List<Dictionary<int, object>> ints = new List<Dictionary<int, object>>();

                Product_Add ap = new Product_Add();
                var get_product = db.Products.Where(m => m.StoreId == storeid).ToList();

                // Fetch VariantTypes and transform them into the intended Dropdownss structure
                var variantTypes = db.VariantTypes
                    .Select(vt => new Dropdown_Fetch
                    {
                        VariantTypeId = vt.VariantTypeId,
                        ProductId = Convert.ToInt32(vt.ProductId),
                        Title = vt.Title,
                        IsVariantLimitSelection = vt.IsVariantLimitSelection,
                        VariantNoOfSelection = vt.VariantNoOfSelection,
                        Options = vt.VariantValues
                            .Select(vv => new DropdownOption
                            {
                                VariantValueId = vv.VariantValueId,
                                Value = vv.Value,
                                VariantImg = vv.VariantImg,

                                VariantPrice = vv.VariantPrice
                            })
                            .ToList()
                    })
                    .ToList();

                // Now, we'll map the product list, including matching the product ID with the variant types

                var productAddList = get_product.Select(product => new Product_AddWithDropdown
                {
                    ProductId = product?.ProductId ?? 0,
                    Name = product?.ProductName ?? "Unknown Product",
                    Description = product?.ProductDesc ?? "No Description",
                    Price = product?.ProductPrice?.ToString() ?? "0",
                    SalesTax = product?.SalesTax?.ToString() ?? "0",
                    Discount = product?.Discount?.ToString() ?? "0",
                    EColumn = product.EColumn,
                    //     WeightId = product.WeightId,
                    IsLimitedSelection = product.IsLimitedSelection,
                    NumberOfSelection = product.NumberOfSelection,
                    MultipleImges = db.ProductsImages.Where(p => p.ProductId == product.ProductId).ToList(),
                    so = product?.SequenceOrder ?? 0,
                    IsActive = product.Isactive,
                    //    WeightPrice = db.WeightDetails.Where(m => m.WeightId == product.WeightId).FirstOrDefault().WeightPrice,
                    //    WeightRange = db.WeightDetails.Where(m => m.WeightId == product.WeightId).FirstOrDefault().WeightRange,
                    Image = product?.ProductImg ?? "No Image",
                    CategoryId = product?.CategoryId ?? 0,
                    Category = db.Categories.Where(m => m.CategoryId == product.CategoryId).Select(m => m.CategoryName).FirstOrDefault(),
                    StoreId = product?.StoreId ?? 0,

                    // Add the dropdown for variant types (these are the available options for the product)
                    VariantTypeDropdowns = db.VariantTypes.Where(m => m.SelectionMode == "Single")
                    .Select(vt => new Dropdown_Fetch
                    {
                        VariantTypeId = vt.VariantTypeId,
                        ProductId = Convert.ToInt32(vt.ProductId),
                        Title = vt.Title,
                        IsVariantLimitSelection = vt.IsVariantLimitSelection,
                        VariantNoOfSelection = vt.VariantNoOfSelection,
                        Options = vt.VariantValues
                            .Select(vv => new DropdownOption
                            {
                                VariantValueId = vv.VariantValueId,
                                Value = vv.Value,
                                VariantImg = vv.VariantImg,

                                VariantPrice = vv.VariantPrice
                            })
                            .ToList()
                    }).Where(m => m.ProductId == product.ProductId)
                    .ToList(),

                    // Default empty addons, you can update this logic to select specific addons if needed


                    Addons = db.VariantTypes.Where(m => m.SelectionMode == "Multiple")
                    .Select(vt => new Addon_Fetch
                    {
                        VariantTypeId = vt.VariantTypeId,
                        ProductId = Convert.ToInt32(vt.ProductId),

                        Title = vt.Title,
                        Products = db.Products
        .Where(m => vt.VariantValues.Select(vv => vv.ProductId).Contains(m.ProductId) && m.EColumn == "Active")
        .Select(p => new Product_Addon
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            ProductDesc = p.ProductDesc,
            ProductPrice = p.ProductPrice,
            SalesTax = p.SalesTax,
            Discount = p.Discount,
            EColumn = p.EColumn,
            isactive = p.Isactive,
            ProductImg = p.ProductImg,
            CategoryId = p.CategoryId.HasValue ? p.CategoryId.Value : 0,
            StoreId = p.StoreId.HasValue ? p.StoreId.Value : 0,
            //    WeightId = p.WeightId,
            //   WeightPrice = db.WeightDetails.Where(w => w.WeightId == p.WeightId).FirstOrDefault().WeightPrice, // Add logic to fetch weight price if needed
            //     WeightRange = db.WeightDetails.Where(w => w.WeightId == p.WeightId).FirstOrDefault().WeightRange, // Add logic to fetch weight range if needed

        })
        .ToList(),

                    }).Where(m => m.ProductId == product.ProductId)
                    .ToList(),

                }).OrderByDescending(c => c.so).Where(m => m.EColumn == "Active").ToList();

                var grr = productAddList
                       .GroupBy(g => g.CategoryId)
                       .OrderBy(g => db.Categories
                           .Where(c => c.CategoryId == g.Key)
                           .Select(c => c.SequenceOrder)
                           .FirstOrDefault()) // Order groups by SequenceOrder
                       .ToDictionary(
                           g => g.Key, // Category ID as Key
                           g => g.ToList() // Convert each group into a list of products
                       ).ToList();



                return Ok(grr);

            }

        }

        [HttpPost("Edit_Item")]
        public async Task<IActionResult> Edit_Item([FromBody] Product_Edit product)
        {
            try
            {
                // Validate input
                if (product == null || string.IsNullOrWhiteSpace(product.name))
                {
                    return BadRequest(new { success = false, message = "Product details are invalid or missing." });
                }
                // Handle category
                int categoryId = categoryId = product.categoryId;

                // Step 2: Handle product image upload
                var productImageUrl = "";
                if (!string.IsNullOrEmpty(product.image))
                {
                    if (product.image.Contains("https"))
                    {
                        productImageUrl = product.image;
                    }
                    else
                    {
                        string base64Image = product.image.Split(',')[1]; // Remove the "data:image/png;base64," prefix
                        byte[] imageBytes = Convert.FromBase64String(base64Image);
                        var stream = new MemoryStream(imageBytes);

                        // Create an IFormFile from the base64 string  // Create a unique file name
                        var fileName = Guid.NewGuid().ToString() + "_product_image.png";

                        // Create an IFormFile from the base64 string
                        var file = new FormFile(stream, 0, imageBytes.Length, "file", fileName)
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = "image/png", // Set the appropriate content type
                            ContentDisposition = $"form-data; name=\"file\"; filename=\"{fileName}\""
                        };
                        // Upload the file to S3 or save it to a folder
                        productImageUrl = await UploadToS3(file);
                    }
                    // Example method to upload to S3
                }
                // Save the product entity to the database
                var getproductfind = await db.Products.FindAsync(product.productId);

                if (getproductfind != null)
                {
                    getproductfind.ProductName = product.name;
                    getproductfind.ProductDesc = product.description;
                    getproductfind.ProductPrice = Convert.ToInt32(product.price);
                    getproductfind.SalesTax = Convert.ToInt32(product.sales_tax);
                    getproductfind.Discount = Convert.ToInt32(product.discount);
                    getproductfind.ProductImg = productImageUrl;
                    getproductfind.IsLimitedSelection = product.IsLimitedSelection;
                    getproductfind.NumberOfSelection = product.NumberOfSelection;
                    getproductfind.CategoryId = categoryId;
                    getproductfind.Isactive = product.IsActive;
                    getproductfind.Stock = product.stock;
                    getproductfind.WeightId = product.WeightId;
                    getproductfind.EColumn = "Active";
                    getproductfind.StoreId = Convert.ToInt32(getproductfind.StoreId); // No need to assign, already exists

                    await db.SaveChangesAsync();
                }

                var getlastproduct = product.productId;


                var getproductimagesrange = db.ProductsImages.Where(p => p.ProductId == product.productId).ToList();
                db.ProductsImages.RemoveRange(getproductimagesrange);
                await db.SaveChangesAsync();

                if (product.MultipleImges != null)
                {
                    foreach (var item in product.MultipleImges)
                    {
                        var MultiproductImageUrl = "";
                        if (!string.IsNullOrEmpty(item.ImageUrl))
                        {
                            if (item.ImageUrl.Contains("https"))
                            {
                                MultiproductImageUrl = item.ImageUrl;
                            }
                            else
                            {
                                string base64Image = item.ImageUrl.Split(',')[1]; // Remove the "data:image/png;base64," prefix
                                byte[] imageBytes = Convert.FromBase64String(base64Image);
                                var stream = new MemoryStream(imageBytes);

                                // Create an IFormFile from the base64 string  // Create a unique file name
                                var fileName = Guid.NewGuid().ToString() + "_product_multi_image.png";

                                // Create an IFormFile from the base64 string
                                var file = new FormFile(stream, 0, imageBytes.Length, "file", fileName)
                                {
                                    Headers = new HeaderDictionary(),
                                    ContentType = "image/png", // Set the appropriate content type
                                    ContentDisposition = $"form-data; name=\"file\"; filename=\"{fileName}\""
                                };
                                // Upload the file to S3 or save it to a folder
                                MultiproductImageUrl = await UploadToS3(file);  // Example method to upload to S3
                            }

                        }
                        ProductsImage productsImage = new ProductsImage();
                        productsImage.ImageUrl = MultiproductImageUrl;
                        productsImage.ProductId = getlastproduct;
                        productsImage.StoreId = getproductfind.StoreId;
                        productsImage.IsActive = 1;
                        db.ProductsImages.Add(productsImage);
                        db.SaveChanges();

                    }
                }


                var getaddonrange = db.VariantTypes.Where(p => p.ProductId == getlastproduct && p.SelectionMode == "Multiple").ToList();


                foreach (var item in getaddonrange)
                {
                    var getaddonvaluerange = db.VariantValues.Where(v => v.VariantTypeId == item.VariantTypeId).ToList();
                    db.RemoveRange(getaddonvaluerange);
                }

                if (getaddonrange.Count > 0)
                {

                    db.RemoveRange(getaddonrange);
                    await db.SaveChangesAsync();
                }


                // Handle Addons
                if (product.Addons != null)
                {
                    foreach (var addonGroup in product.Addons)
                    {
                        var variantType = new VariantType
                        {
                            StoreId = getproductfind.StoreId,
                            ProductId = getlastproduct,
                            SelectionMode = "Multiple",
                            Title = addonGroup.Key
                        };
                        db.VariantTypes.Add(variantType);
                        await db.SaveChangesAsync();

                        foreach (var addon in addonGroup.Value)
                        {
                            var variantValue = new VariantValue
                            {
                                VariantTypeId = variantType.VariantTypeId,
                                Value = addon.productName,
                                ProductId = addon.productId,
                                StoreId = getproductfind.StoreId
                            };
                            db.VariantValues.Add(variantValue);
                        }
                    }
                }


                // Get all Variant Types based on ProductId
                var getVariantTypes = db.VariantTypes.Where(v => v.ProductId == getlastproduct && v.SelectionMode == "Single").ToList();

                foreach (var item in getVariantTypes)
                {

                    var getVariantValues = db.VariantValues.Where(m => m.VariantTypeId == item.VariantTypeId).ToList();
                    db.VariantValues.RemoveRange(getVariantValues);
                }


                // Delete all Variant Types
                if (getVariantTypes.Any())
                {
                    db.VariantTypes.RemoveRange(getVariantTypes);
                }

                // Save changes to the database

                await db.SaveChangesAsync();

                // Handle Dropdowns
                if (product.variantTypeDropdowns != null)
                {
                    foreach (var dropdownGroup in product.variantTypeDropdowns)
                    {
                        var dr = dropdownGroup.Value.FirstOrDefault();

                        var variantType = new VariantType
                        {
                            StoreId = product.storeId,
                            ProductId = getlastproduct,
                            SelectionMode = "Single",
                            Title = dropdownGroup.Key,
                            IsVariantLimitSelection = dr.IsVariantLimitSelection,
                            VariantNoOfSelection = dr.VariantNoOfSelection

                        };
                        db.VariantTypes.Add(variantType);
                        await db.SaveChangesAsync();

                        foreach (var dropdown in dropdownGroup.Value)
                        {
                            var dropdownImageUrl = "";

                            if (!string.IsNullOrEmpty(dropdown.Image))
                            {

                                if (dropdown.Image.Contains("https"))
                                {
                                    dropdownImageUrl = dropdown.Image;
                                }
                                else
                                {
                                    string base64Image = dropdown.Image.Split(',')[1]; // Remove the "data:image/png;base64," prefix
                                    byte[] imageBytes = Convert.FromBase64String(base64Image);
                                    var stream = new MemoryStream(imageBytes);

                                    // Create an IFormFile from the base64 string  // Create a unique file name
                                    var fileName = Guid.NewGuid().ToString() + "_product_image.png";

                                    // Create an IFormFile from the base64 string
                                    var file = new FormFile(stream, 0, imageBytes.Length, "file", fileName)
                                    {
                                        Headers = new HeaderDictionary(),
                                        ContentType = "image/png", // Set the appropriate content type
                                        ContentDisposition = $"form-data; name=\"file\"; filename=\"{fileName}\""
                                    };
                                    // Upload the file to S3 or save it to a folder
                                    dropdownImageUrl = await UploadToS3(file);  // Example method to upload to S3
                                }


                            }

                            var variantValue = new VariantValue
                            {
                                VariantTypeId = variantType.VariantTypeId,
                                Value = dropdown.Name,
                                VariantImg = dropdownImageUrl,
                                VariantPrice = dropdown.Price,
                                StoreId = product.storeId
                            };
                            db.VariantValues.Add(variantValue);
                        }
                    }
                }
                await db.SaveChangesAsync();
                return Ok(new { success = true, message = "Product Edited successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // ORDER MANAGEMENT START//////////////////////////////////////

        [HttpPost("fetch_orders")]
        [Authorize]
        public IActionResult fetch_orders(int store_id, string order_status)
        {

            if (order_status == "All_Orders")
            {

                gro gr = new gro();
                var getorder = db.OrderMasters
                  .Select(m => new OrderMaster
                  {
                      Mid = m.Mid,
                      OrderId = m.OrderId,
                      CustomerId = m.CustomerId,
                      Quantity = m.Quantity,
                      TotalPrice = m.TotalPrice,
                      OrderStatus = m.OrderStatus,
                      TransactionDt = m.TransactionDt,
                      EColumn = m.EColumn,
                      AddedBy = m.AddedBy,
                      StoreId = m.StoreId,
                      IsCancel = m.IsCancel,
                      CancelReason = m.CancelReason,
                      IsApproved = m.IsApproved,
                      OrderType = m.OrderType,
                      CustomerInstruction = m.CustomerInstruction,
                      Address = m.Address,
                      City = m.City,
                      ProvinceId = m.ProvinceId,
                      CityId = m.CityId,
                      CountryId = m.CountryId,
                      SalesTax = m.SalesTax,
                    
                      //  ShippingMethods = m.ShippingMethods,
                      ShippingPrice = m.ShippingPrice,
                      Discount = m.Discount,
                      PaymentMethod = m.PaymentMethod,
                      PaymentMethodName = m.PaymentMethodName,
                      ItemPrice = m.ItemPrice,
                      IsSeen = m.IsSeen,
                      TotalWeight = m.TotalWeight,
                      LastUpdateDate = m.LastUpdateDate,
                      OrderDetails = db.OrderDetails.Where(p => p.Mid == m.Mid)
                      .Select(o => new OrderDetail
                      {
                          Did = o.Did,
                          Mid = o.Mid,

                          StoreId = o.StoreId,
                          EColumn = o.Product.ProductName,
                          ProductImage = o.ProductImage,
                          Price = o.Price,
                          WeightRange = o.WeightRange,
                          WeightId = o.WeightId,
                          WeightPrice = o.WeightPrice,
                          ProductId = o.ProductId,
                          ProductItem = o.ProductItem,
                          TransactionDt = o.TransactionDt,
                          OrderDetailSubDetails = db.OrderDetailSubDetails.Where(d => d.Did == o.Did).ToList()
                      }).ToList(),
                      Customer = db.Customers.Where(c => c.CustomerId == m.CustomerId).FirstOrDefault(),

                      // Assuming OrderDetails is already a collection.
                  }).Where(x => x.StoreId == store_id)

                  .ToList();

                return Ok(getorder);
            }
            else
                if (order_status != "All_Orders")
            {

                gro gr = new gro();
                var getorder = db.OrderMasters
                  .Select(m => new OrderMaster
                  {
                      Mid = m.Mid,
                      OrderId = m.OrderId,
                      CustomerId = m.CustomerId,
                      Quantity = m.Quantity,
                      TotalPrice = m.TotalPrice,
                      OrderStatus = m.OrderStatus,
                      TransactionDt = m.TransactionDt,
                      EColumn = m.EColumn,
                      AddedBy = m.AddedBy,
                      StoreId = m.StoreId,
                      IsCancel = m.IsCancel,
                      OrderType = m.OrderType,
                      CustomerInstruction = m.CustomerInstruction,
                      CancelReason = m.CancelReason,
                      IsApproved = m.IsApproved,
                      Address = m.Address,
                      City = m.City,
                      SalesTax=m.SalesTax,
                      //  ShippingMethods = m.ShippingMethods,
                      ShippingPrice = m.ShippingPrice,
                      Discount = m.Discount,
                      PaymentMethod = m.PaymentMethod,
                      PaymentMethodName = m.PaymentMethodName,
                      ItemPrice = m.ItemPrice,
                      TotalWeight = m.TotalWeight,
                      IsSeen = m.IsSeen,
                      LastUpdateDate = m.LastUpdateDate,
                      OrderDetails = db.OrderDetails.Where(p => p.Mid == m.Mid)
                      .Select(o => new OrderDetail
                      {
                          Did = o.Did,
                          Mid = o.Mid,

                          StoreId = o.StoreId,
                          EColumn = o.Product.ProductName,
                          Price = o.Price,
                          ProductImage = o.ProductImage,
                          ProductId = o.ProductId,
                          ProductItem = o.ProductItem,
                          TransactionDt = o.TransactionDt,
                          WeightId = o.WeightId,
                          WeightRange = o.WeightRange,
                          WeightPrice = o.WeightPrice,
                          OrderDetailSubDetails = db.OrderDetailSubDetails.Where(d => d.Did == o.Did).ToList()
                      }).ToList(),
                      Customer = db.Customers.Where(c => c.CustomerId == m.CustomerId).FirstOrDefault(),

                      // Assuming OrderDetails is already a collection.
                  }).Where(x => x.StoreId == store_id && x.OrderStatus == order_status)

                  .ToList();

                return Ok(getorder);
            }
            else
            {
                return Ok(new { notfound = "notfound" });
            }
        }
        [HttpPost("Delete_Order_By_Id")]
        public IActionResult Delete_Order_By_Id(int id, int store_id)
        {

            try
            {


                var find_order_master = db.OrderMasters.Where(m => m.Mid == id && m.StoreId == store_id).FirstOrDefault();
                if (find_order_master != null)
                {

                    var find_order_detail = db.OrderDetails.Where(m => m.Mid == find_order_master.Mid && m.StoreId == store_id).ToList();
                    if (find_order_detail.Any())
                    {
                        foreach (var order_detail in find_order_detail)
                        {
                            var find_order_subdetail = db.OrderDetailSubDetails.Where(s => s.Did == order_detail.Did && s.StoreId == store_id).ToList();
                            if (find_order_subdetail.Any())
                            {
                                db.OrderDetailSubDetails.RemoveRange(find_order_subdetail);
                            }
                        }
                        db.OrderDetails.RemoveRange(find_order_detail);
                    }
                    db.OrderMasters.Remove(find_order_master);
                    db.SaveChanges();

                    return Ok(new { success = "Order Deleted Sucessfully..." });
                }
                else
                {
                    return Ok(new { Alert = "Order Not Found..." });
                }

            }
            catch (Exception e)
            {

                return Ok(new { error = "Error while deleting Order " + e.Message });
            }

        }




        [HttpPost("summary_dashboard")]
        //   [Authorize]
        public IActionResult summary(int store_id)
        {
            // Initialize the summary object
            Summary summary = new Summary
            {
                Daily = new PeriodData
                {
                    Labels = new List<string>(),
                    Values = new List<int>()
                },
                Monthly = new PeriodData
                {
                    Labels = new List<string>(),
                    Values = new List<int>()
                },
                Yearly = new PeriodData
                {
                    Labels = new List<string>(),
                    Values = new List<int>()
                }
            };


            var findstore = db.Stores.Find(store_id);
            if (findstore != null)
            {
                // Populate Daily Labels
                DateTime sdt = PST();

                DateTime previous_day = PST().AddDays(-1);
                DateTime previous_month = PST().AddMonths(-1);
                DateTime previous_year = PST().AddYears(-1);




                // Populate daily labels and values
                for (int i = -4; i < 1; i++)
                {
                    // Calculate the date
                    DateTime date = sdt.AddDays(i);

                    // Add day name to labels
                    summary.Daily.Labels.Add(date.ToString("ddd"));

                    // Fetch matching order from the database
                    var order = db.OrderMasters.Where(m => EF.Functions.DateDiffDay(m.TransactionDt, date) == 0 && m.StoreId == store_id && m.OrderStatus == "Dispatch").Select(s => s.TotalPrice).Sum();
                    int getvalue = order != null ? Convert.ToInt32(order) : 0;
                    summary.Daily.Values.Add(getvalue);

                    summary.Daily.Total = summary.Daily.Total + getvalue;

                    var order_cod = db.OrderMasters.Where(m => EF.Functions.DateDiffDay(m.TransactionDt, PST()) == 0 && m.StoreId == store_id && m.PaymentMethodName == "COD" && m.OrderStatus == "Dispatch").Select(s => s.TotalPrice).Sum();
                    int getvalue_cod = order_cod != null ? Convert.ToInt32(order_cod) : 0;

                    summary.Daily.Cash = getvalue_cod;

                    var order_online = db.OrderMasters.Where(m => EF.Functions.DateDiffDay(m.TransactionDt, PST()) == 0 && m.StoreId == store_id && m.PaymentMethodName == "Online" && m.OrderStatus == "Dispatch").Select(s => s.TotalPrice).Sum();
                    int getvalue_online = order_online != null ? Convert.ToInt32(order_online) : 0;

                    summary.Daily.Online = getvalue_online;


                    //previous day

                    var pre_order_cod = db.OrderMasters.Where(m => EF.Functions.DateDiffDay(m.TransactionDt, previous_day) == 0 && m.StoreId == store_id && m.PaymentMethodName == "COD" && m.OrderStatus == "Dispatch").Select(s => s.TotalPrice).Sum();
                    int pre_getvalue_cod = order_cod != null ? Convert.ToInt32(pre_order_cod) : 0;

                    summary.Daily.previous_cod = pre_getvalue_cod;

                    var pre_order_online = db.OrderMasters.Where(m => EF.Functions.DateDiffDay(m.TransactionDt, previous_day) == 0 && m.StoreId == store_id && m.PaymentMethodName == "Online" && m.OrderStatus == "Dispatch").Select(s => s.TotalPrice).Sum();
                    int pre_getvalue_online = order_online != null ? Convert.ToInt32(pre_order_online) : 0;
                    summary.Daily.previous_online = pre_getvalue_online;

                    // Add value to daily values

                }

                // Populate Monthly Labels
                for (int i = -4; i <= 0; i++)
                {
                    DateTime month = sdt.AddMonths(i); // Adjust the date by i months
                    summary.Monthly.Labels.Add(month.ToString("MMM ")); // Add month names

                    var order = db.OrderMasters.Where(m => EF.Functions.DateDiffMonth(m.TransactionDt, month) == 0 && m.StoreId == store_id && m.OrderStatus == "Dispatch").Select(s => s.TotalPrice).Sum();
                    int getvalue = order != null ? Convert.ToInt32(order) : 0;
                    summary.Monthly.Values.Add(getvalue);

                    summary.Monthly.Total = summary.Monthly.Total + getvalue;

                    var order_cod = db.OrderMasters.Where(m => EF.Functions.DateDiffMonth(m.TransactionDt, PST()) == 0 && m.StoreId == store_id && m.PaymentMethodName == "COD" && m.OrderStatus == "Dispatch").Select(s => s.TotalPrice).Sum();
                    int getvalue_cod = order_cod != null ? Convert.ToInt32(order_cod) : 0;
                    summary.Monthly.Cash = getvalue_cod;

                    var order_online = db.OrderMasters.Where(m => EF.Functions.DateDiffMonth(m.TransactionDt, PST()) == 0 && m.StoreId == store_id && m.PaymentMethodName == "Online" && m.OrderStatus == "Dispatch").Select(s => s.TotalPrice).Sum();
                    int getvalue_online = order_online != null ? Convert.ToInt32(order_online) : 0;
                    summary.Monthly.Online = getvalue_online;

                    //previous month

                    var pre_order_cod = db.OrderMasters.Where(m => EF.Functions.DateDiffDay(m.TransactionDt, previous_month) == 0 && m.StoreId == store_id && m.PaymentMethodName == "COD" && m.OrderStatus == "Dispatch").Select(s => s.TotalPrice).Sum();
                    int pre_getvalue_cod = order_cod != null ? Convert.ToInt32(pre_order_cod) : 0;

                    summary.Monthly.previous_cod = pre_getvalue_cod;

                    var pre_order_online = db.OrderMasters.Where(m => EF.Functions.DateDiffDay(m.TransactionDt, previous_month) == 0 && m.StoreId == store_id && m.PaymentMethodName == "Online" && m.OrderStatus == "Dispatch").Select(s => s.TotalPrice).Sum();
                    int pre_getvalue_online = order_online != null ? Convert.ToInt32(pre_order_online) : 0;
                    summary.Monthly.previous_online = pre_getvalue_online;

                }

                // Populate Yearly Labels
                int currentYear = sdt.Year;
                for (int i = -4; i <= 0; i++)
                {
                    summary.Yearly.Labels.Add((currentYear + i).ToString()); // Add last 5 years
                    DateTime year = sdt.AddYears(i);

                    var order = db.OrderMasters.Where(m => EF.Functions.DateDiffYear(m.TransactionDt, year) == 0 && m.StoreId == store_id && m.OrderStatus == "Dispatch").Select(s => s.TotalPrice).Sum();
                    int getvalue = order != null ? Convert.ToInt32(order) : 0;
                    summary.Yearly.Values.Add(getvalue);

                    summary.Yearly.Total = summary.Yearly.Total + getvalue;

                    var order_cod = db.OrderMasters.Where(m => EF.Functions.DateDiffYear(m.TransactionDt, PST()) == 0 && m.StoreId == store_id && m.PaymentMethodName == "COD" && m.OrderStatus == "Dispatch").Select(s => s.TotalPrice).Sum();
                    int getvalue_cod = order_cod != null ? Convert.ToInt32(order_cod) : 0;
                    summary.Yearly.Cash = getvalue_cod;

                    var order_online = db.OrderMasters.Where(m => EF.Functions.DateDiffYear(m.TransactionDt, PST()) == 0 && m.StoreId == store_id && m.PaymentMethodName == "Online" && m.OrderStatus == "Dispatch").Select(s => s.TotalPrice).Sum();
                    int getvalue_online = order_online != null ? Convert.ToInt32(order_online) : 0;
                    summary.Yearly.Online = getvalue_online;

                    //previous year

                    var pre_order_cod = db.OrderMasters.Where(m => EF.Functions.DateDiffDay(m.TransactionDt, previous_year) == 0 && m.StoreId == store_id && m.PaymentMethodName == "COD" && m.OrderStatus == "Dispatch").Select(s => s.TotalPrice).Sum();
                    int pre_getvalue_cod = order_cod != null ? Convert.ToInt32(pre_order_cod) : 0;

                    summary.Yearly.previous_cod = pre_getvalue_cod;

                    var pre_order_online = db.OrderMasters.Where(m => EF.Functions.DateDiffDay(m.TransactionDt, previous_year) == 0 && m.StoreId == store_id && m.PaymentMethodName == "Online" && m.OrderStatus == "Dispatch").Select(s => s.TotalPrice).Sum();
                    int pre_getvalue_online = order_online != null ? Convert.ToInt32(pre_order_online) : 0;
                    summary.Yearly.previous_online = pre_getvalue_online;

                }

                // Fetch orders from the database (if used later)
                var get_orders = db.OrderMasters.ToList();

                // Return response (modify as needed)
                return Ok(summary);
            }
            else
            {
                return Ok(new { alert = "No Store Found" });
            }
        }

        // SUMMARY END ////////////////----//////////////////////////

        // STORE MANAGEMENT START////////////////////////////

        [HttpPost("Add_Store")]
        public async Task<IActionResult> Add_Store([FromBody] Add_Store store)
        {

            var productImageUrl = "";
            if (!string.IsNullOrEmpty(store.store.StoreImg))
            {
                string base64Image = store.store.StoreImg.Split(',')[1]; // Remove the "data:image/png;base64," prefix
                byte[] imageBytes = Convert.FromBase64String(base64Image);
                var stream = new MemoryStream(imageBytes);

                // Create an IFormFile from the base64 string  // Create a unique file name
                var fileName = Guid.NewGuid().ToString() + "_store_image.png";

                // Create an IFormFile from the base64 string
                var file = new FormFile(stream, 0, imageBytes.Length, "file", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/png", // Set the appropriate content type
                    ContentDisposition = $"form-data; name=\"file\"; filename=\"{fileName}\""
                };
                // Upload the file to S3 or save it to a folder
                productImageUrl = await UploadToS3(file);  // Example method to upload to S3

            }
            store.store.StoreImg = productImageUrl;
            store.store.TransactionDt = PST();
            store.store.Isactive = "Active";

            db.Stores.Add(store.store);
            db.SaveChanges();
            var get_store_id = db.Stores.Where(s => s.StoreLink == store.store.StoreLink && s.OwnerCnic == store.store.OwnerCnic && s.OwnerName == store.store.OwnerName).AsNoTracking().ToList().Last().StoreId;
            AccessDetail accessDetail = new AccessDetail();
            accessDetail.AccessLink = "https://" + store.store.StoreLink;
            accessDetail.StoreId = get_store_id;
            accessDetail.AccessStatus = "Active";
            db.AccessDetails.Add(accessDetail);
            db.SaveChanges();

            byte[] secrated = new byte[64];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(secrated);
            }

            string secretekey = Convert.ToBase64String(secrated);

            // Skip the first 4 characters and take the next 8 characters
            string shortKey = new string(secretekey.Skip(4).Take(8).ToArray());

            if (store.ExcelData.Count > 0)
            {
                store.ExcelData.ForEach(x => x.StoreId = get_store_id);
                store.ExcelData.ForEach(x => x.TransactionDt = PST());

            }
            db.AriaDetails.AddRange(store.ExcelData);
            db.SaveChanges();



            User user = new User();

            user.Useremail = store.store.StoreName;
            user.StoreId = get_store_id;
            user.Isactive = "Active";
            user.Region = store.store.StoreLocation;
            user.UserName = store.store.OwnerName;
            user.UserPassword = shortKey;
            user.TransactionDt = PST();
            db.Users.Add(user);
            db.SaveChanges();

            SendEmail(store.store.OwnerEmail, "Welcome to Tapppp - Store Details", store.store.OwnerName, user.Useremail, user.UserPassword);
            return Ok(new { success = "Store Created Successfully...", Credentials_For = store.store.StoreName, UserName = user.Useremail, UserPassword = user.UserPassword });
        }



        [HttpGet("Edit_Store")]

        public IActionResult Edit_Store(int store_id)
        {


            var findstore = db.Stores.Find(store_id);
            if (findstore != null)
            {
                Fetch_Store fetch_Store = new Fetch_Store
                {
                    StoreId = findstore.StoreId,
                    StoreName = findstore.StoreName,
                    WebType = findstore.WebType,
                    StoreImg = findstore.StoreImg,
                    StoreLocation = findstore.StoreLocation,
                    StoreContact = findstore.StoreContact,
                    AddedBy = findstore.AddedBy,
                    TransactionDt = findstore.TransactionDt,
                    EColumn = findstore.EColumn,
                    UserId = findstore.UserId,
                    StoreLink = findstore.StoreLink,
                    Isactive = findstore.Isactive,
                    CoverImage = findstore.CoverImage?.ToString().Split(',') ?? new string[0], // Converting comma-separated images to array
                    CommisionShipmentPrice = findstore.CommisionShipmentPrice,
                    ShipmentMode = findstore.ShipmentMode,
                    StoreCity = findstore.StoreCity,
                    OwnerCnic = findstore.OwnerCnic,
                    PickupAddress = findstore.PickupAddress,
                    OwnerName = findstore.OwnerName,
                    OwnerEmail = findstore.OwnerEmail,
                    CityId = findstore.CityId,
                    ProvinceId = findstore.ProvinceId,
                    CountryIdId = findstore.CountryIdId,
                    BusinessType = findstore.BusinessType,
                    FacebookLink = findstore.FacebookLink,
                    InstagramLink = findstore.InstagramLink,
                    LinkedinLink = findstore.LinkedinLink,
                    WhatsappLink = findstore.WhatsappLink,
                    FacebookPixelLink = findstore.FacebookPixelLink,
                    ExtraColumn = findstore.ExtraColumn,
                    TextColor = findstore.TextColor,
                    BackgroundColor = findstore.BackgroundColor,
                    TagLine = findstore.TagLine,
                    FreeDeliveryAmount=findstore.FreeDeliveryAmount,
                    IsOpen=findstore.IsOpen,
                    SalesTax=findstore.SalesTax,
                    ButtonColor=findstore.ButtonColor

                };



                var finduser = db.Users.Where(m => m.StoreId == store_id).FirstOrDefault();

                return Ok(new { Store_Detail = fetch_Store, Credentials_Details = finduser });
            }
            else
            {
                return Ok("Store Not Found");
            }


        }

        [HttpPost("Change_Password")]

        public IActionResult Change_Password(User user)
        {

            var finduser = db.Users.Where(m => m.StoreId == user.StoreId).FirstOrDefault();

            if (finduser != null)
            {

                finduser.Useremail = user.Useremail;

                finduser.UserPassword = user.UserPassword;
                db.Users.Update(finduser);
                db.SaveChanges();


                return Ok(new { alert = "credentials updated sucessfully..." });
            }
            else
            {
                return Ok("Not Found");
            }


        }

        [HttpPost("Update_Store")]
        public async Task<IActionResult> Update_Store([FromBody] Edit_Store edit_Store)
        {
            var findstore = db.Stores.Find(edit_Store.store_id);
            if (findstore == null)
            {
                return Ok(new { Success = "Store Not Found..." });
            }

            // Update Store details
            findstore.StoreName = edit_Store.businessName;
            findstore.StoreLocation = edit_Store.store_location;
            findstore.OwnerCnic = edit_Store.cnic;
            findstore.StoreContact = edit_Store.phone;
            findstore.OwnerEmail = edit_Store.email;
            findstore.OwnerName = edit_Store.owner_name;
            findstore.FacebookLink = edit_Store.facebookUrl;
            findstore.WhatsappLink = edit_Store.whatsappUrl;
            findstore.InstagramLink = edit_Store.instagramUrl;
            findstore.TagLine = edit_Store.marketingTagline;
            findstore.LinkedinLink = edit_Store.linkedInUrl;
            findstore.FacebookPixelLink = edit_Store.facebookpixel;
            findstore.TextColor = edit_Store.text_color;
            findstore.BackgroundColor = edit_Store.background_color;

            findstore.FreeDeliveryAmount = edit_Store.FreeDeliveryAmount;
            findstore.IsOpen = edit_Store.IsOpen;
            findstore.SalesTax = edit_Store.SalesTax;
            findstore.ButtonColor = edit_Store.ButtonColor;

            // Handle logo upload
            if (!string.IsNullOrEmpty(edit_Store.logo))
            {
                findstore.StoreImg = await ProcessImage(edit_Store.logo);
            }

            // Handle product images upload (store as comma-separated values)
            if (edit_Store.productImages.Length > 0)
            {
                List<string> uploadedImages = new List<string>();
                foreach (var item in edit_Store.productImages)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        uploadedImages.Add(await ProcessImage(item));
                    }
                }
                findstore.CoverImage = string.Join(",", uploadedImages); // Store all images as a comma-separated string
            }



            db.Stores.Update(findstore);
            db.SaveChanges();

            return Ok(new { Success = "Store Updated Successfully..." });
        }

        // Method to process image (checks if it's a URL or needs base64 conversion)
        private async Task<string> ProcessImage(string imageBase64OrUrl)
        {
            if (imageBase64OrUrl.Contains("https"))
            {
                return imageBase64OrUrl; // Return URL directly
            }

            try
            {
                string base64Image = imageBase64OrUrl.Split(',')[1]; // Remove prefix
                byte[] imageBytes = Convert.FromBase64String(base64Image);
                var stream = new MemoryStream(imageBytes);

                var fileName = Guid.NewGuid().ToString() + "_store_image.png";

                var file = new FormFile(stream, 0, imageBytes.Length, "file", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/png",
                    ContentDisposition = $"form-data; name=\"file\"; filename=\"{fileName}\""
                };

                return await UploadToS3(file); // Upload and return URL
            }
            catch
            {
                return ""; // Return empty string if conversion fails
            }
        }






        [HttpPost("upload_files")]
        public IActionResult UploadFiles(Upload_Files upload_Files)
        {
            try
            {
                if (upload_Files.Area_Details?.Any() == true)
                {
                    int storeId = upload_Files.store_id;

                    var findexistingareas = db.AriaDetails.Where(m => m.StoreId == storeId);
                    if (findexistingareas.Any())
                    {
                        db.AriaDetails.RemoveRange(findexistingareas);
                    }

                    upload_Files.Area_Details.ForEach(x => x.TransactionDt = PST());
                    upload_Files.Area_Details.ForEach(x => x.StoreId = storeId);
                    db.AriaDetails.AddRange(upload_Files.Area_Details);
                }

                if (upload_Files.Pickup_Details?.Any() == true)
                {
                    var storeId = upload_Files.store_id;

                    var findexistingpickup = db.PickupDetails.Where(m => m.StoreId == storeId);
                    if (findexistingpickup.Any())
                    {
                        db.PickupDetails.RemoveRange(findexistingpickup);
                    }

                    upload_Files.Pickup_Details.ForEach(x => x.TransactionDt = PST());
                    upload_Files.Pickup_Details.ForEach(x => x.StoreId = storeId);
                    db.PickupDetails.AddRange(upload_Files.Pickup_Details);
                }

                db.SaveChanges();
                return Ok(new { Success = "Files Uploaded Sucessfully..." });
            }
            catch (Exception e)
            {

                return BadRequest(new { error = "error while uploading files " + e + "" });
            }

        }



        [HttpPost("domain_check")]
        public IActionResult domain_checker(string domain_name)
        {

            var verify = db.Stores.Where(s => s.StoreLink == domain_name).FirstOrDefault();
            if (verify != null)
            {
                return Ok(new { alert = "Exist" });
            }
            else
            {
                return Ok(new { alert = "No Exist" });
            }

        }

        [HttpPost("sendemail")]
        public IActionResult SendEmail(string recipientEmail, string subject, string ownername, string username, string password)
        {
            string senderEmail = "support@tapppp.com";
            string senderPassword = "Tapppp24";
            string smtpServer = "w1.hosterpk.com";
            int smtpPort = 465;

            // Build the HTML content directly as a string with inline CSS
            string body = @"
<!DOCTYPE html>
<html>

<head>
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f9f9f9;
            margin: 0;
            padding: 0;
        }
        p,li {
    font-family: system-ui;
    font-size: 14px;
}
li{
    margin-top: 10px;
    color: #555;
}
h2 {
    margin-top: 62px;
    font-size: 20px;
    margin-bottom: 0px;
    font-family: system-ui;
}

        .email-container {
            max-width: 800px;
            margin: 20px auto;
            background-color: #ffffff;
            border: 1px solid #ddd;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }

        .email-header {
            background-color: #0056b3;
            color: #ffffff;
            text-align: center;
            padding: 20px;
        }

        .email-header h1 {
            margin: 0;
            font-size: 24px;
        }

        .email-body {
            padding: 20px;
        }

        .email-body p {
            line-height: 1.6;
            color: #555;
        }

        .email-body .highlight {
            color: #0056b3;
            font-weight: bold;
        }

        .email-footer {
            background-color: #f2f2f2;
            text-align: center;
            padding: 15px;
            font-size: 12px;
            color: #888;
        }

        .button {
            display: inline-block;
            margin: 20px 0;
            padding: 10px 20px;
            color: white;
            background-color: #0056b3;
            border-radius: 5px;
            text-decoration: none;
            font-size: 14px;
        }

        .button:hover {
            background-color: black;
            color: white;
        }

        @media screen and (max-width: 600px) {
            .email-body {
                padding: 15px;
            }

            .button {
                padding: 10px 15px;
                font-size: 12px;
            }
        }
        a{
            text-decoration: none;
        }
    </style>
</head>
<body>
    <div class='email-container'>

        <div class='email-body'>
            <p style='margin:0px;'><strong> Dear " + ownername + ",</strong></p> <p style='margin-top: 0px;'> Thank you for signing up your store on <span class='highlight'>Tapppp!</span></p> <p>Here are your store credentials:</p> <ul>    <li><strong>Username:</strong> " + username + "</li>    <li><strong>Password:</strong> " + password + "</li>  </ul> <h2>What’s Next?</h2>   <p style='margin-top:0px;'>Your store will be live within 1-3 hours.Once it’s live, we will notify you via email with all the         details.</p>   <p><strong>Instructions for Setup:</strong></p>       <ol>         <li>Log in to your dashboard at <a href='https://admin.tapppp.com'                 class='highlight'>admin.tapppp.com</a>.</li>         <li>Start adding your categories, products, and set pricing as needed.</li>      <li>If you need assistance, contact our support team at <a href='mailto:support@tapppp.com' class='highlight'>support           @tapppp.com</a> or call us at +923390000127.</li>       </ol>         <p>We’re excited to have you on board and look forward to seeing your store thrive!</p>      <br>      <b style='line-height: 25px;' >Tapppp</b>     <br>     <b style='line-height: 25px;'>Tapppp.com</b>    <br>       <a style='line-height: 25px;' href='mailto:support@tapppp.com' ><strong>support@tapppp.com</strong> </a>      <br>    <p style='margin:0px;'> <strong> +923390000127</strong></p>      </div>  </div></body></html>";

            var emailsetting = _configuration.GetSection("EmailSetting");
            var ccEmail = emailsetting["cc_email"]; // Get cc_email directly from config
            var ccEmaill = emailsetting["ccemail"]; // Get cc_email directly from config


            // Create the email message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Tapppp", senderEmail));
            message.To.Add(new MailboxAddress("Recipient", recipientEmail));
            message.Cc.Add(new MailboxAddress("Recipient", ccEmail)); // Add CC email directly
            message.Cc.Add(new MailboxAddress("Recipient", ccEmaill));

            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            try
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect(smtpServer, smtpPort, true);
                    client.Authenticate(senderEmail, senderPassword);
                    client.Send(message);
                    client.Disconnect(true);
                }
                return Ok(new { success = true, message = "Email sent successfully!" });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Error sending email: " + ex.Message });
            }
        }

        // STORE MANAGEMENT END////////////////////////////

        [HttpPost("Active_InActive_Product")]
        public IActionResult Active_InActive_Product(int product_id, string status)
        {

            var fetch_product = db.Products.Find(product_id);
            if (fetch_product != null)
            {
                fetch_product.Isactive = status;

                db.Products.Update(fetch_product);

                if (status == "Active")
                {
                    db.SaveChanges();
                    return Ok(new { message = "Product Activated Successfully" });
                }
                else
                if (status == "InActive")
                {
                    db.SaveChanges();
                    return Ok(new { message = "Product In-Activated Successfully" });
                }
                else
                {
                    return Ok(new { message = "Undefined Status!" });
                }
            }

            return Ok();
        }

        [HttpDelete("Delete_Store_By_Id")]
        public IActionResult Delete_Store_By_Id(int id)
        {

            try
            {


                var find_store = db.Stores.Where(m => m.StoreId == id).FirstOrDefault();
                if (find_store != null)
                {


                    var find_order_master = db.OrderMasters.Where(m => m.StoreId == id).FirstOrDefault();
                    if (find_order_master != null)
                    {

                        var find_order_detail = db.OrderDetails.Where(m => m.StoreId == id).ToList();
                        if (find_order_detail.Any())
                        {
                            foreach (var order_detail in find_order_detail)
                            {
                                var find_order_subdetail = db.OrderDetailSubDetails.Where(m => m.StoreId == id).ToList();
                                if (find_order_subdetail.Any())
                                {
                                    db.OrderDetailSubDetails.RemoveRange(find_order_subdetail);
                                }
                            }
                            db.OrderDetails.RemoveRange(find_order_detail);
                        }
                        db.OrderMasters.Remove(find_order_master);
                        db.SaveChanges();


                    }



                    var find_product_images = db.ProductsImages.Where(m => m.StoreId == id).AsNoTracking().ToList();
                    if (find_product_images.Any())
                    {

                        db.ProductsImages.RemoveRange(find_product_images);
                    }

                    var find_variant_value = db.VariantValues.Where(m => m.StoreId == id).AsNoTracking().ToList();
                    if (find_variant_value.Any())
                    {

                        db.VariantValues.RemoveRange(find_variant_value);
                    }

                    var find_variant_type = db.VariantTypes.Where(m => m.StoreId == id).AsNoTracking().ToList();
                    if (find_variant_type.Any())
                    {

                        db.VariantTypes.RemoveRange(find_variant_type);
                    }

                    var find_product = db.Products.Where(m => m.StoreId == id).AsNoTracking().ToList();
                    if (find_product.Any())
                    {

                        db.Products.RemoveRange(find_product);
                    }


                    var find_category = db.Categories.Where(m => m.StoreId == id).AsNoTracking().ToList();
                    if (find_category.Any())
                    {

                        db.Categories.RemoveRange(find_category);
                    }

                    var find_delivery = db.AriaDetails.Where(m => m.StoreId == id).AsNoTracking().ToList();
                    if (find_delivery.Any())
                    {

                        db.AriaDetails.RemoveRange(find_delivery);
                    }
                    var find_pickup = db.PickupDetails.Where(m => m.StoreId == id).AsNoTracking().ToList();
                    if (find_pickup.Any())
                    {

                        db.PickupDetails.RemoveRange(find_pickup);
                    }

                    var stid = id.ToString();
                    var find_login_history = db.LoginHistories.Where(m => m.StoreId == stid).AsNoTracking().ToList();
                    if (find_login_history.Any())
                    {

                        db.LoginHistories.RemoveRange(find_login_history);
                    }


                    var find_customer = db.Customers.Where(m => m.StoreId == id).AsNoTracking().ToList();
                    if (find_customer.Any())
                    {

                        db.Customers.RemoveRange(find_customer);
                    }


                    var find_user = db.Users.Where(m => m.StoreId == id).AsNoTracking().ToList();
                    if (find_user.Any())
                    {

                        db.Users.RemoveRange(find_user);
                    }

                    var find_access = db.AccessDetails.Where(m => m.StoreId == id).AsNoTracking().ToList();
                    if (find_access.Any())
                    {

                        db.AccessDetails.RemoveRange(find_access);
                    }





                    db.Stores.Remove(find_store);
                    db.SaveChanges();

                    return Ok(new { success = "Store Deleted Sucessfully..." });
                }
                else
                {
                    return Ok(new { Alert = "Store Not Found..." });
                }

            }
            catch (Exception e)
            {

                return Ok(new { error = "Error while deleting Store " + e.Message });
            }

        }




        [HttpPost("Sync_Domain")]
        public IActionResult Sync_Domain(string url,int store_id) {

            AccessDetail accessDetail = new AccessDetail();
            accessDetail.AccessLink = url;
            accessDetail.StoreId = store_id;
            accessDetail.AccessStatus = "Active";
            db.AccessDetails.Add(accessDetail);
            db.SaveChanges();

            return Ok(new { Success = "Domain Synced Successfully.." });
        
        }


        //[HttpPost("test")]
        //public IActionResult savs()
        //{


        //    string excelFilePath = @"C:\Users\owais\Downloads\Restaurants Menu - Data Scrape (1).xlsx";  // Update this path
        //    string connectionStringSqlServer = "Server=66.165.248.146;Database=tapppp_store_db;User Id=tapppp_dbs;Password=Tapppp24;Trusted_Connection=False;MultipleActiveResultSets=false;TrustServerCertificate=True";

        //    // Connection to read Excel file
        //    string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + excelFilePath + @";Extended Properties='Excel 12.0;HDR=YES;IMEX=1';";

        //    DataTable excelData = GetExcelData(excelConnectionString);

        //    // Insert Categories into the Categories table
        //    InsertCategories(excelData, connectionStringSqlServer, 112);

        //    // Insert Products into the Products table
        //    InsertProducts(excelData, connectionStringSqlServer, 112);

        //    return Ok();

        //}

        static DataTable GetExcelData(string excelConnectionString)
        {
            using (OleDbConnection excelConn = new OleDbConnection(excelConnectionString))
            {
                excelConn.Open();
                OleDbDataAdapter dataAdapter = new OleDbDataAdapter("SELECT * FROM [Sangat$]", excelConn);  // Adjust sheet name if needed
                DataTable dt = new DataTable();
                dataAdapter.Fill(dt);
                return dt;
            }
        }

        static void InsertCategories(DataTable excelData, string connectionStringSqlServer, int store_id)
        {
            using (SqlConnection conn = new SqlConnection(connectionStringSqlServer))
            {
                conn.Open();


                foreach (DataRow row in excelData.Rows)
                {
                    string categoryName = row["Category "].ToString();
                    if (!string.IsNullOrEmpty(categoryName))
                    {
                        // First, check if the category already exists
                        string checkCategoryQuery = "SELECT COUNT(*) FROM [dbo].[Categories] WHERE [category_name] = @category_name and store_id=" + store_id + " and category_desc='Active'";

                        using (SqlCommand checkCmd = new SqlCommand(checkCategoryQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@category_name", categoryName);

                            int categoryCount = (int)checkCmd.ExecuteScalar();

                            // If the category does not exist, insert it
                            if (categoryCount == 0)
                            {
                                string insertCategoryQuery = "INSERT INTO [dbo].[Categories] ([category_name], [category_desc], [category_img], [transaction_dt], [store_id]) " +
                                                             "VALUES (@category_name, 'Active', NULL, GETDATE(), " + store_id + ")";

                                using (SqlCommand insertCmd = new SqlCommand(insertCategoryQuery, conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@category_name", categoryName);
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                }
            }
        }

        static void InsertProducts(DataTable excelData, string connectionStringSqlServer, int store_id)
        {
            using (SqlConnection conn = new SqlConnection(connectionStringSqlServer))
            {
                conn.Open();

                foreach (DataRow row in excelData.Rows)
                {
                    string productName = row["Product Name"].ToString();
                    string productDescription = row["Product Description"].ToString();
                    string url = row["URL"].ToString();
                    decimal price = Convert.ToDecimal(row["Price"]);
                    string categoryName = row["Category "].ToString();

                    // Get category_id from Categories table
                    string categoryIdQuery = "SELECT category_id FROM [dbo].[Categories] WHERE category_name = @category_name AND store_id =" + store_id + " and category_desc='Active' ";
                    int categoryId = 0;

                    using (SqlCommand cmd = new SqlCommand(categoryIdQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@category_name", categoryName);
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            categoryId = Convert.ToInt32(result);
                        }
                    }

                    // Insert the product into the Products table
                    string insertProductQuery = "INSERT INTO [dbo].[Products] ([product_name], [product_desc], [store_id], [product_img], [category_id], " +
                                                "[product_price], [added_by], [e_column], [transaction_dt], [sales_tax], [discount], " +
                                                "[stock], [weight_id], [isactive], [is_limited_selection], [number_of_selection]) " +
                                                "VALUES (@product_name, @product_desc, " + store_id + ", @product_img, @category_id, @product_price, NULL, 'Active', GETDATE(), NULL, NULL, NULL, NULL, 'Active', 0, NULL)";
                    using (SqlCommand cmd = new SqlCommand(insertProductQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@product_name", productName);
                        cmd.Parameters.AddWithValue("@product_desc", productDescription);
                        cmd.Parameters.AddWithValue("@product_img", url);
                        cmd.Parameters.AddWithValue("@category_id", categoryId);
                        cmd.Parameters.AddWithValue("@product_price", price);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

        }



        [HttpPost("update_product_sequence")]
        public IActionResult update_product_sequence(int productid, int storeid, int new_position, int category_id)
        {

            try
            {
                db.Database.ExecuteSqlRaw("exec UpdateProductSequence @p0,@p1,@p2,@p3", productid, category_id, new_position, storeid);

                return Ok("Updated");
            }
            catch (Exception e)
            {

                return BadRequest("Error"+ e);
            }
       

        }

        [HttpPost("update_category_sequence")]
        public IActionResult update_category_sequence(int storeid, int new_position, int category_id)
        {
            try
            {
                db.Database.ExecuteSqlRaw("exec UpdateCategorySequence @p0,@p1,@p2", category_id, new_position, storeid);
                return Ok("Category Updated ");

            }
            catch (Exception e)
            {
                return BadRequest("Error"+ e);
                
            }
            

        }






    }


}

















