using System.Collections.Generic;
using System.Linq;

namespace Depanneur.App.Models
{
    public class UserRecap
    {
        public string BaseUrl {get; set; }

        public string Name { get; set; }

        public Week RecapWeek { get; set; }
        public Week PreviousWeek { get; set; }

        public List<SummaryItem> Purchases { get; set; }

        public decimal CurrentBalance { get; set; }

        public decimal RecapTotal => Purchases.Sum(p => p.RecapTotal);
        public decimal PreviousTotal => Purchases.Sum(p => p.PreviousTotal);
        public decimal TotalDelta => RecapTotal - PreviousTotal;
    }
}