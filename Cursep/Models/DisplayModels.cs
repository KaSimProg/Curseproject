using System;

namespace Cursep.Models
{
    // Классы для отображения данных в DataGrid
    public class MaterialDisplay
    {
        public int MaterialID { get; set; }
        public string MaterialName { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public string SupplierName { get; set; }
        public decimal UnitPrice { get; set; }
        public string UnitOfMeasure { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }
    }

    public class StockDisplay
    {
        public string MaterialName { get; set; }
        public string CategoryName { get; set; }
        public int Quantity { get; set; }
        public string Location { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class UserDisplay
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string RoleName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class OperationDisplay
    {
        public DateTime OperationDate { get; set; }
        public string UserName { get; set; }
        public string OperationType { get; set; }
        public string MaterialName { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; }
    }

    public class ReportDisplay
    {
        public string MaterialName { get; set; }
        public int Income { get; set; }
        public int Outcome { get; set; }
        public int Balance { get; set; }
        public decimal TotalValue { get; set; }
    }
}