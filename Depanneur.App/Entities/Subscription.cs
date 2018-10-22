using System.ComponentModel.DataAnnotations.Schema;

namespace Depanneur.App.Entities
{
    public class Subscription
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public int ProductId { get; set; }

        public int Quantity { get; set; }
        public SubscriptionFrequency Frequency { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }

    public enum SubscriptionFrequency {
        Daily = 1,
        Weekly = 2
    }
}