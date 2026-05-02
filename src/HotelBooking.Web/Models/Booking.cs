namespace HotelBooking.Web.Models;

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }

    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null;

        public int RoomId { get; set; }
        public Room Room { get; set; } = null;

        public DateOnly CheckIn  { get; set; }
        public DateOnly CheckOut  { get; set; }

        public int GuestsCount { get; set; }
        public string? SpecialRequests { get; set; } // Guest requestests

        public BookingStatus Status { get; set; } =  BookingStatus.Pending;

        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Calculation: PricePerNight
        public int NightsCount => CheckOut.DayNumber -  CheckIn.DayNumber;
}
