namespace Server.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public string EAN { get; set; }

        public int ManufacturedAt { get; set; }

        public double Price { get; set; }

        public string Description { get; set; }
    }
}
