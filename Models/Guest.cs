
namespace WeddingPlanner.Models
{
    public class Guest
    {
        public int GuestId { get; set; }
        public int UserId { get; set; }
        public int WeddingId { get; set; }

        //Navigation objects

        public User GuestUser { get; set; }
        public Wedding ForWedding { get; set; }
    }
}