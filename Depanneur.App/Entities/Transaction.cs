using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Depanneur.App.Entities
{
    public abstract class Transaction
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }
        public decimal Amount { get; set; }

        public decimal NewBalance { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
