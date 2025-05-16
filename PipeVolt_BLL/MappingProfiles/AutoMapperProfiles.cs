using AutoMapper;
using PipeVolt_DAL.DTOS;
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

    }
}
