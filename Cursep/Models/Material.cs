namespace Cursep.Models
{
    public class Material
    {
        public int MaterialID { get; set; }
        public string MaterialName { get; set; }
        public string Description { get; set; }
        public int CategoryID { get; set; }
        public int SupplierID { get; set; }
        public decimal UnitPrice { get; set; }
        public string UnitOfMeasure { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }

        // Навигационные свойства
        public Category Category { get; set; }
        public Supplier Supplier { get; set; }
        public Stock Stock { get; set; }
    }
}