using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
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
