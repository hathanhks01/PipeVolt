using Microsoft.EntityFrameworkCore;
using PipeVolt_DAL;
using PipeVolt_DAL.IRepositories;
using PipeVolt_DAL.Repositories;
using PipeVolt_BLL.IServices;
using PipeVolt_BLL.Services;
using PipeVolt_DAL.Models;
using PipeVolt_Api.Common.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
        };
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
    options.EnableDetailedErrors = true; // Cho phép lỗi chi tiết trong SignalR
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
// Logger
builder.Services.AddScoped<ILoggerService, LoggerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapHub<ChatHub>("/chathub");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseSession();
app.UseCors("AllowReactApp");
app.MapControllers();

app.Run();
