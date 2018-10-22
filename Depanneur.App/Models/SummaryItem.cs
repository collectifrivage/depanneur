namespace Depanneur.App.Models
{
    public class SummaryItem
    {
        public string ProductName { get; set; }

        public int RecapCount { get; set; }
        public decimal RecapTotal { get; set; }

        public int PreviousCount { get; set; }
        public decimal PreviousTotal { get; set; }

        public int CountDelta => RecapCount - PreviousCount;
        public decimal TotalDelta => RecapTotal - PreviousTotal;

        public bool IsSubscribed { get; internal set; }
    }
}