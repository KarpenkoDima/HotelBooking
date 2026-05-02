namespace HotelBooking.Web.Models
{
    public class RoomType
    {
        public int Id { get; set; }
        public required string Name { get; set; } // "Standerd", "Delux", "Suite"
        public required string Description { get; set; }

        public ICollection<Room> Rooms { get; set; } = [];
    }
}
