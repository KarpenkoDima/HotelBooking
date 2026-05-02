using HotelBooking.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Web.Data;

    public class AppDbContext(DbContextOptions options) :DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Booking> Bookings =>  Set<Booking>();
        public DbSet<Room> Rooms =>  Set<Room>();
        public DbSet<RoomType> RoomTypes =>  Set<RoomType>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === User ===
            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique(); // Email - unique
                e.Property(u => u.Email).HasMaxLength(256);
                e.Property(u => u.PasswordHash).HasMaxLength(256);
                e.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("User");
            });

            // === Room ===
            modelBuilder.Entity<Room>(e =>
            {
                e.Property(r => r.Number).IsUnicode();
                e.Property(r => r.Number).HasMaxLength(10);
                e.Property(r => r.PricePerNight).HasColumnType("decimal(18,2)");
            });

            // === Booking ===
            modelBuilder.Entity<Booking>(e =>
            {
                e.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
                e.Property(b => b.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                // Business-rules: CheckOut > CheckIn
                e.ToTable(t => t.HasCheckConstraint(
                    "CK_Booking_Checkout_After_CheckIn",
                    "\"CheckOut\" > \"CheckIn\""));
            });
        }
    }
