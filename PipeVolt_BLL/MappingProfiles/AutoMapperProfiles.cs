using AutoMapper;
using PipeVolt_DAL.DTOS;
using PipeVolt_DAL.DTOS.PipeVolt_DAL.DTOS;
using PipeVolt_DAL.Models;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Brand, BrandDto>();
        CreateMap<BrandDto, Brand>();
        CreateMap<CreateBrandDto, Brand>();
        CreateMap<UpdateBrandDto, Brand>();

        CreateMap<Product, ProductDto>()
           .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
           .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.BrandName));

        // DTO → Entity (mới thêm)
        CreateMap<CreateProductDto, Product>();

        CreateMap<UpdateProductDto, Product>()
           // bỏ map ProductId nếu bạn không muốn DTO ghi đè
           .ForMember(dest => dest.ProductId, opt => opt.Ignore());
        CreateMap<Customer, CustomerDto>();
        CreateMap<CreateCustomerDto, Customer>()
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.RegistrationDate, opt => opt.Ignore());
        CreateMap<UpdateCustomerDto, Customer>()
            .ForMember(dest => dest.RegistrationDate, opt => opt.Ignore());

        CreateMap<ProductCategory, ProductCategoryDto>();
        CreateMap<CreateProductCategoryDto, ProductCategory>()
            .ForMember(dest => dest.CategoryId, opt => opt.Ignore());
        CreateMap<UpdateProductCategoryDto, ProductCategory>();
        CreateMap<Employee, EmployeeDto>();
        CreateMap<CreateEmployeeDto, Employee>()
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore());
        CreateMap<UpdateEmployeeDto, Employee>();

        // Inventory
        CreateMap<Inventory, InventoryDto>();
        CreateMap<CreateInventoryDto, Inventory>()
            .ForMember(dest => dest.InventoryId, opt => opt.Ignore());
        CreateMap<UpdateInventoryDto, Inventory>();

        // OrderDetail
        CreateMap<OrderDetail, OrderDetailDto>();
        CreateMap<CreateOrderDetailDto, OrderDetail>()
            .ForMember(dest => dest.OrderDetailId, opt => opt.Ignore());
        CreateMap<UpdateOrderDetailDto, OrderDetail>();

        // PurchaseOrder
        CreateMap<PurchaseOrder, PurchaseOrderDto>();
        CreateMap<CreatePurchaseOrderDto, PurchaseOrder>()
            .ForMember(dest => dest.PurchaseOrderId, opt => opt.Ignore());
        CreateMap<UpdatePurchaseOrderDto, PurchaseOrder>();

        // PurchaseOrderDetail
        CreateMap<PurchaseOrderDetail, PurchaseOrderDetailDto>();
        CreateMap<CreatePurchaseOrderDetailDto, PurchaseOrderDetail>()
            .ForMember(dest => dest.PurchaseOrderDetailId, opt => opt.Ignore());
        CreateMap<UpdatePurchaseOrderDetailDto, PurchaseOrderDetail>();

        // SalesOrder
        CreateMap<SalesOrder, SalesOrderDto>();
        CreateMap<CreateSalesOrderDto, SalesOrder>()
            .ForMember(dest => dest.OrderId, opt => opt.Ignore());
        CreateMap<UpdateSalesOrderDto, SalesOrder>();

        // Supplier
        CreateMap<Supplier, SupplierDto>();
        CreateMap<CreateSupplierDto, Supplier>()
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore());
        CreateMap<UpdateSupplierDto, Supplier>();

        // Supply
        CreateMap<Supply, SupplyDto>();
        CreateMap<CreateSupplyDto, Supply>()
            .ForMember(dest => dest.SupplyId, opt => opt.Ignore());
        CreateMap<UpdateSupplyDto, Supply>();

        // Warehouse
        CreateMap<Warehouse, WarehouseDto>();
        CreateMap<CreateWarehouseDto, Warehouse>()
            .ForMember(dest => dest.WarehouseId, opt => opt.Ignore());
        CreateMap<UpdateWarehouseDto, Warehouse>();

        // Warranty
        CreateMap<Warranty, WarrantyDto>();
        CreateMap<CreateWarrantyDto, Warranty>()
            .ForMember(dest => dest.WarrantyId, opt => opt.Ignore());
        CreateMap<UpdateWarrantyDto, Warranty>();
        // Ánh xạ mới cho Cart
        CreateMap<Cart, CartDto>()
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()) // Tổng sẽ được tính trong service
            .ForMember(dest => dest.CartItems, opt => opt.MapFrom(src => src.CartItems));
        CreateMap<CreateCartDto, Cart>()
            .ForMember(dest => dest.CartId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<UpdateCartDto, Cart>()
            .ForMember(dest => dest.CartId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Ánh xạ mới cho CartItem
        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName));
        CreateMap<AddCartItemDto, CartItem>()
            .ForMember(dest => dest.CartItemId, opt => opt.Ignore())
            .ForMember(dest => dest.CartId, opt => opt.Ignore())
            .ForMember(dest => dest.UnitPrice, opt => opt.Ignore()) // UnitPrice sẽ được gán trong service
            .ForMember(dest => dest.LineTotal, opt => opt.Ignore()); // Tính tự động trong DB
        CreateMap<UpdateCartItemDto, CartItem>()
            .ForMember(dest => dest.CartItemId, opt => opt.Ignore())
            .ForMember(dest => dest.CartId, opt => opt.Ignore())
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
            .ForMember(dest => dest.LineTotal, opt => opt.Ignore());
        // Thêm vào AutoMapperProfiles constructor
        CreateMap<UserAccount, UserAccountDto>();
        CreateMap<CreateUserAccountDto, UserAccount>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore());
        CreateMap<UpdateUserAccountDto, UserAccount>()
    .ForMember(dest => dest.UserId, opt => opt.Ignore())
    .ForMember(dest => dest.Password, opt =>
        opt.Condition(src => !string.IsNullOrWhiteSpace(src.Password)));

    }
}
