using System;

namespace Cursep.Models
{
    public class Stock
    {
        public int StockID { get; set; }
        public int MaterialID { get; set; }
        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Location { get; set; }
        public Material Material { get; set; }
    }
}