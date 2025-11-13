using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace StoreTeddyBear.Models;

public partial class StorepinkteddybearBdContext : DbContext
{
    public StorepinkteddybearBdContext()
    {
    }

    public StorepinkteddybearBdContext(DbContextOptions<StorepinkteddybearBdContext> options)
        : base(options)
    {
    }

    public static StorepinkteddybearBdContext Instance { get; } = new();


    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Orderitem> Orderitems { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Toy> Toys { get; set; }

    public virtual DbSet<Useransadmin> Useransadmins { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;username=root;password=1234;database=storepinkteddybear_bd", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.43-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.IdInventory).HasName("PRIMARY");

            entity.ToTable("inventory");

            entity.HasIndex(e => e.ArticulToy, "ArticulToy");

            entity.Property(e => e.ArticulToy).HasMaxLength(50);

            entity.HasOne(d => d.ArticulToyNavigation).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.ArticulToy)
                .HasConstraintName("inventory_ibfk_1");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.IdOrder).HasName("PRIMARY");

            entity.ToTable("order");

            entity.Property(e => e.AdressOrder).HasMaxLength(500);
            entity.Property(e => e.DateOrder)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.StatusOrder)
                .HasDefaultValueSql("'pending'")
                .HasColumnType("enum('pending','processing','shipped','delivered','cancelled')");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'");
        });

        modelBuilder.Entity<Orderitem>(entity =>
        {
            entity.HasKey(e => e.IdOrderItem).HasName("PRIMARY");

            entity.ToTable("orderitem");

            entity.HasIndex(e => e.ArticulToy, "ArticulToy");

            entity.HasIndex(e => e.IdOrder, "IdOrder");

            entity.Property(e => e.ArticulToy).HasMaxLength(50);
            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);

            entity.HasOne(d => d.ArticulToyNavigation).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.ArticulToy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orderitem_ibfk_2");

            entity.HasOne(d => d.IdOrderNavigation).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.IdOrder)
                .HasConstraintName("orderitem_ibfk_1");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.IdReview).HasName("PRIMARY");

            entity.ToTable("review");

            entity.HasIndex(e => e.ArticulToy, "ArticulToy");

            entity.HasIndex(e => e.IdCustomer, "IdCustomer");

            entity.Property(e => e.ArticulToy).HasMaxLength(50);
            entity.Property(e => e.CommentReview).HasColumnType("text");
            entity.Property(e => e.DateReview)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.ArticulToyNavigation).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ArticulToy)
                .HasConstraintName("review_ibfk_1");

            entity.HasOne(d => d.IdCustomerNavigation).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.IdCustomer)
                .HasConstraintName("review_ibfk_2");
        });

        modelBuilder.Entity<Toy>(entity =>
        {
            entity.HasKey(e => e.ArticulToy).HasName("PRIMARY");

            entity.ToTable("toy");

            entity.Property(e => e.ArticulToy).HasMaxLength(50);
            entity.Property(e => e.Descriptionn).HasColumnType("text");
            entity.Property(e => e.Height).HasMaxLength(10);
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Weight).HasMaxLength(10);
        });

        modelBuilder.Entity<Useransadmin>(entity =>
        {
            entity.HasKey(e => e.IdCustomer).HasName("PRIMARY");

            entity.ToTable("useransadmin");

            entity.HasIndex(e => e.EmailUsers, "EmailCustomer").IsUnique();

            entity.Property(e => e.NameUsers).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.RoleUsers).HasMaxLength(45);
            entity.Property(e => e.StatusUsersProfile).HasMaxLength(45);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
