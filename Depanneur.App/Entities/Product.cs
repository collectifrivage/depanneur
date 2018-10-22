using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Depanneur.App.Entities
{
    public class Product : ISoftDeletable
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public bool IsSubscribable { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Purchase> Purchases { get; set; }
    }
}
