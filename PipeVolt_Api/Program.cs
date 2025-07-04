﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PipeVolt_Api.Common.Repository;
using PipeVolt_Api.Configurations;
using PipeVolt_BLL.IServices;
using PipeVolt_BLL.Services;
using PipeVolt_DAL;
using PipeVolt_DAL.IRepositories;
using PipeVolt_DAL.Models;
using PipeVolt_DAL.Repositories;
using System.Text;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<PipeVoltDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});


builder.Services.AddDistributedMemoryCache(); // Lưu trữ session trong bộ nhớ
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thiết lập thời gian hết hạn của session
});
builder.Services.AddMemoryCache();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // For debugging
    options.MaximumReceiveMessageSize = 32768; // 32KB
    options.StreamBufferCapacity = 10;
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);
});
// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddScoped<IGenericRepository<UserAccount>, UserAccountRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGenericRepository<Customer>, CustomerRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IGenericRepository<Product>, ProductRepository>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IGenericRepository<Brand>, BrandRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IGenericRepository<Customer>, CustomerRepository>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IGenericRepository<ProductCategory>, ProductCategoryRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IGenericRepository<Employee>, EmployeeRepository>();
builder.Services.AddScoped<IGenericRepository<Inventory>, InventoryRepository>();
builder.Services.AddScoped<IGenericRepository<OrderDetail>, OrderDetailRepository>();
builder.Services.AddScoped<IGenericRepository<PurchaseOrder>, PurchaseOrderRepository>();
builder.Services.AddScoped<IGenericRepository<PurchaseOrderDetail>, PurchaseOrderDetailRepository>();
builder.Services.AddScoped<IGenericRepository<SalesOrder>, SalesOrderRepository>();
builder.Services.AddScoped<IGenericRepository<Supplier>, SupplierRepository>();
builder.Services.AddScoped<IGenericRepository<Supply>, SupplyRepository>();
builder.Services.AddScoped<IGenericRepository<Warehouse>, WarehouseRepository>();
builder.Services.AddScoped<IGenericRepository<Warranty>, WarrantyRepository>();
builder.Services.AddScoped<IGenericRepository<Cart>, CartRepository>();
builder.Services.AddScoped<IGenericRepository<CartItem>, CartItemRepository>();
builder.Services.AddScoped<ICartRepository,CartRepository>();
builder.Services.AddScoped<ICartItemRepository,CartItemRepository>();
builder.Services.AddScoped<IGenericRepository<Invoice>, InvoiceRepository>();
builder.Services.AddScoped<IGenericRepository<InvoiceDetail>, InvoiceDetailRepository>();
builder.Services.AddScoped<IGenericRepository<PaymentMethod>, PaymentMethodRepository>();
builder.Services.AddScoped<IGenericRepository<PaymentTransaction>, PaymentTransactionRepository>();
// Services
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IPurchaseOrderDetailService, PurchaseOrderDetailService>();
builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ISupplyService, SupplyService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IWarrantyService, WarrantyService>();
builder.Services.AddScoped<ICartItemService, CartItemService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();
builder.Services.AddScoped<ICheckoutService,CheckoutService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IAIChatbotService, AIChatbotService>();
// Logger
builder.Services.AddScoped<ILoggerService, LoggerService>();
// Đăng ký JWT và Policy Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddGoogleAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();


builder.Services.AddHttpClient();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");
app.MapHub<ChatHub>("/chathub");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseSession();
app.MapControllers();

app.Run();
