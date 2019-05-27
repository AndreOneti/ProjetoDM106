using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetoDM106.Models
{
    public class Order
    {
        public Order()
        {
            this.OrderItems = new HashSet<OrderItem>();
        }

        public int Id { get; set; }

        public string userEmail { get; set; }

        public DateTime DateOrder { get; set; }

        public DateTime DateDelivery { get; set; }

        public string Status { get; set; }

        public float ItemPrice { get; set; }

        public float ItemWeight { get; set; }

        public decimal precoFrete { get; set; }

        public virtual ICollection<OrderItem> OrderItems{ get; set; }
    }
}