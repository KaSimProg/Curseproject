using System;

namespace Cursep.Models
{
    public class Operation
    {
        public int OperationID { get; set; }
        public int MaterialID { get; set; }
        public int UserID { get; set; }
        public string OperationType { get; set; } // INCOME/OUTCOME
        public int Quantity { get; set; }
        public DateTime OperationDate { get; set; }
        public string ReferenceNumber { get; set; }
        public string Notes { get; set; }

        public Material Material { get; set; }
        public User User { get; set; }
    }
}