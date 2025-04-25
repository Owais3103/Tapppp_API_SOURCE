using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography;
using System.Text;
using virtual_store.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
byte[] secrated = new byte[64];
using (var random = RandomNumberGenerator.Create())
{
    random.GetBytes(secrated);
}
string secretekey = Convert.ToBase64String(secrated);

// Register DbContext
builder.Services.AddDbContext<VirtualStoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("v_store"))); // Replace with your connection string
builder.Services.AddHostedService<CorsOriginUpdater>();

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            // Dynamically check if origin is allowed
            return CorsOriginUpdater.AllowedOrigins.Contains(origin);
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();

    });
});


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<VirtualStoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("v_store"))); // Replace with your connection string


// Configure JWT Bearer Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Ensure the token was issued by "https://example.com"
            ValidateAudience = true, // Ensure the token is for "https://example.com/users"
            ValidateLifetime = true, // Ensure the token has not expired
            ValidateIssuerSigningKey = true, // Validate the signature using the key
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"], // "https://example.com"
            ValidAudience = builder.Configuration["JwtSettings:Audience"], // "https://example.com/users"
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])) // "MySuperSecretKey12345"
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();
builder.Services.AddScoped<OrderService>(); // Register OrderService



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//builder.Services.AddSwaggerGen(options =>
//{
//    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Virtual Store API", Version = "v1" });

//    // Add JWT Authorization header configuration
//    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "Enter 'Bearer' [space] and then your valid JWT token in the text input below.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsIn..."
//    });

//    options.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            Array.Empty<string>()
//        }
//    });
//});




app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI();
// Use Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();



app.MapControllers();


app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<OrderHub>("/orderHub");
});
app.Run();
