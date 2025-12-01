using System;

namespace QuanLyPhongTro.DTO
{
    public class CustomerDebtDTO
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public decimal TotalDebt { get; set; }
        public int UnpaidInvoiceCount { get; set; }
        public DateTime? LastInvoiceDate { get; set; }

        public string DebtDisplay => TotalDebt > 0 ? $"{TotalDebt:N0} đ" : "Không nợ";
        public bool HasDebt => TotalDebt > 0;
    }
}
