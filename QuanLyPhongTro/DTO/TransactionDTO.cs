using System;

namespace QuanLyPhongTro.DTO
{
    public class TransactionDTO
    {
        public int TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }  // 'Thu', 'Chi'
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public decimal Amount { get; set; }
        public int? RoomId { get; set; }
        public int? CustomerId { get; set; }
        public int? InvoiceId { get; set; }
        public string Description { get; set; }
        public string PaymentMethod { get; set; }
        public string ReceivedBy { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        // Display
        public string RoomName { get; set; }
        public string CustomerName { get; set; }

        public string TypeDisplay => TransactionType == "Thu" ? "Thu" : "Chi";
        public string AmountDisplay => $"{(TransactionType == "Thu" ? "+" : "-")}{Amount:N0}";
        public string PaymentMethodDisplay
        {
            get
            {
                switch (PaymentMethod)
                {
                    case "TienMat": return "Tiền mặt";
                    case "ChuyenKhoan": return "Chuyển khoản";
                    case "MoMo": return "MoMo";
                    default: return PaymentMethod;
                }
            }
        }
    }

    public class TransactionCategoryDTO
    {
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string TransactionType { get; set; }
        public bool IsSystem { get; set; }
        public int SortOrder { get; set; }
    }

    public class TransactionSummary
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance => TotalIncome - TotalExpense;
        public int IncomeCount { get; set; }
        public int ExpenseCount { get; set; }
    }
}
