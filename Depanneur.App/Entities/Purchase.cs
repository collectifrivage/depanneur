using System.ComponentModel.DataAnnotations.Schema;

namespace Depanneur.App.Entities
{
    public class Purchase : Transaction
    {
        public decimal ProductPrice { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }

        public bool IsFromSubscription { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        public int ProductId { get; set; }
    }
}
