namespace HotelBooking.Web.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        // BCrypt hash, never plain text
        public required string PasswordHash { get; set; }

        public required string FirstName { get; set; }
        public required string LastName { get; set; }

        // Role: User and Admin
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; }=DateTime.UtcNow;

        // Navigation property EF Core
        public ICollection<Booking> Bookings { get; set; } =[];

        public string FullName => $"{FirstName} {LastName}";
    }
}
