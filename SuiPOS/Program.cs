using ECommerceMVC.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.Services.Implementations;
using SuiPOS.Services.Interfaces;
using SuiPOS.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ? ADD SESSION SUPPORT
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<SuiPosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Cloudinary settings from appsettings.json
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

// Register Services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IAuthService, AuthService>();

//Register Customer Services
builder.Services.AddScoped<ICustomerService, CustomerService>();

//Register Category Services
builder.Services.AddScoped<ICategoryService, CategoryService>();

//Register Attribute Services
builder.Services.AddScoped<IAttributeService, AttributeService>();

//Register SystemSetting Services
builder.Services.AddScoped<ISystemSettingService, SystemSettingService>();

//Register Promotion Services
builder.Services.AddScoped<IPromotionService, PromotionService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register JwtHelper
builder.Services.AddScoped<JwtHelper>();

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["suipos_ac"];
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
name: "default",
pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
