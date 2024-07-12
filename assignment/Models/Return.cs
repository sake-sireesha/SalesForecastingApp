namespace SalesForecastingApp.Models
{
    public class Return
    {
        public int ReturnID { get; set; }
        public int OrderID { get; set; }
        public DateTime ReturnDate { get; set; }
        public int Quantity { get; set; }
        public decimal Sales { get; set; }
    }
}
