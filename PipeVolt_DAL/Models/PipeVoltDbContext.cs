using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PipeVolt_DAL.Models;

public partial class PipeVoltDbContext : DbContext
{
    public PipeVoltDbContext()
    {
    }

    public PipeVoltDbContext(DbContextOptions<PipeVoltDbContext> options)
        : base(options)
    {
    }
    public virtual DbSet<Cart> Carts { get; set; }
    public virtual DbSet<CartItem> CartItems { get; set; }
    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }

    public virtual DbSet<SalesOrder> SalesOrders { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<Supply> Supplies { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    public virtual DbSet<Warranty> Warranties { get; set; }
    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public virtual DbSet<Invoice> Invoices { get; set; }
    public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=MSI\\SQLEXPRESS;Database=PipeVolt;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__BRAND__5E5A8E2748CDEFCA");

            entity.Property(e => e.BrandId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__CUSTOMER__CD65CB85C2818D2A");

            entity.Property(e => e.CustomerId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__EMPLOYEE__C52E0BA83BED5CBA");

            entity.Property(e => e.EmployeeId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("PK__INVENTOR__B59ACC491540FD43");

            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Product).WithMany(p => p.Inventories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__INVENTORY__produ__5629CD9C");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.Inventories).HasConstraintName("FK__INVENTORY__purch__571DF1D5");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.Inventories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__INVENTORY__wareh__5535A963");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__ORDER_DE__3C5A4080964198A2");

            entity.Property(e => e.LineTotal).HasComputedColumnSql("([quantity]*[unit_price]-[discount])", false);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_DET__order__693CA210");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ORDER_DET__produ__6A30C649");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__PRODUCT__47027DF521C9A1DD");

            entity.Property(e => e.ProductId).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCT__brand_i__440B1D61");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCT__categor__4316F928");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__PRODUCT___D54EE9B4968BF011");

            entity.Property(e => e.CategoryId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.PurchaseOrderId).HasName("PK__PURCHASE__AFCA88E6D458B84D");

            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Employee).WithMany(p => p.PurchaseOrders).HasConstraintName("FK__PURCHASE___emplo__5165187F");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseOrders).HasConstraintName("FK__PURCHASE___suppl__5070F446");
        });

        modelBuilder.Entity<PurchaseOrderDetail>(entity =>
        {
            entity.HasKey(e => e.PurchaseOrderDetailId).HasName("PK__PURCHASE__4B370F6D511BCF68");

            entity.Property(e => e.LineTotal).HasComputedColumnSql("([quantity]*[unit_cost])", false);

            entity.HasOne(d => d.Product).WithMany(p => p.PurchaseOrderDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PURCHASE___produ__6E01572D");

            entity.HasOne(d => d.PurchaseOrder).WithMany(p => p.PurchaseOrderDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PURCHASE___purch__6D0D32F4");
        });

        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__SALES_OR__4659622993289FF9");

            entity.Property(e => e.NetAmount).HasComputedColumnSql("(([total_amount]-[discount_amount])+[tax_amount])", false);
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Customer).WithMany(p => p.SalesOrders).HasConstraintName("FK__SALES_ORD__custo__6477ECF3");

            entity.HasOne(d => d.Employee).WithMany(p => p.SalesOrders).HasConstraintName("FK__SALES_ORD__emplo__656C112C");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__SUPPLIER__6EE594E812C79687");

            entity.Property(e => e.SupplierId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Supply>(entity =>
        {
            entity.HasKey(e => e.SupplyId).HasName("PK__SUPPLY__4870CD837B5C9A2E");

            entity.HasOne(d => d.Product).WithMany(p => p.Supplies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SUPPLY__product___48CFD27E");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Supplies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SUPPLY__supplier__49C3F6B7");
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__USER_ACC__B9BE370FE3A6149D");

            entity.HasOne(d => d.Customer).WithMany(p => p.UserAccounts).HasConstraintName("FK__USER_ACCO__custo__5FB337D6");

            entity.HasOne(d => d.Employee).WithMany(p => p.UserAccounts).HasConstraintName("FK__USER_ACCO__emplo__5EBF139D");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.WarehouseId).HasName("PK__WAREHOUS__734FE6BFE44CB61E");

            entity.Property(e => e.WarehouseId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Warranty>(entity =>
        {
            entity.HasKey(e => e.WarrantyId).HasName("PK__WARRANTY__24E65B0430326D8D");

            entity.HasOne(d => d.Customer).WithMany(p => p.Warranties)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WARRANTY__custom__71D1E811");

            entity.HasOne(d => d.Product).WithMany(p => p.Warranties)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WARRANTY__produc__70DDC3D8");
        });
        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PK__PAYMENT_METHOD__E8C9B7A3");
            entity.Property(e => e.PaymentMethodId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__PAYMENT_TRANSACTION__A1C3D8F2");
            entity.HasOne(d => d.Order)
                .WithMany(p => p.PaymentTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PAYMENT_TRANS__order__7A672E12");
            entity.HasOne(d => d.PaymentMethod)
                .WithMany(p => p.PaymentTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PAYMENT_TRANS__method__7B573F34");
        });
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__INVOICE__ID");
            entity.Property(e => e.InvoiceId).ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Order).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.Customer).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.HasKey(e => e.InvoiceDetailId).HasName("PK__INVOICE_DETAIL__ID");
            entity.Property(e => e.LineTotal)
                .HasComputedColumnSql("(([quantity]*[unit_price])-isnull([discount],0))", false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
