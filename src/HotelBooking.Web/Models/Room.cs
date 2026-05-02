namespace HotelBooking.Web.Models
{
    public class Room
    {
        public int Id { get; set; }

        public required string Number { get; set; } // "101" "202A"
        public required string Name { get; set; } // "Standard" "Сщьащке"
        public required string Description { get; set; }

        public int Capacity { get; set; } // capacity of guests
        public decimal PricePerNight { get; set; }

        public int Floor { get; set; }
        public bool IsActive { get; set; } = true;

        // Foreign Key + Navigation
        public int RoomTypeId { get; set; }
        public RoomType? RoomType { get; set; } = null;

        public ICollection<Booking> Bookings { get; set; } = [];
    }
}
