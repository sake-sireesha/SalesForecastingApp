namespace SalesForecastingApp.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public int CustomerID { get; set; }
        public string? State { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal Sales { get; set; }

    }
}
