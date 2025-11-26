using System;

namespace QuanLyPhongTro.DTO
{
    public class InvoiceDTO
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        // Chỉ số điện
        public int ElecOld { get; set; }
        public int ElecNew { get; set; }
        public decimal ElecPrice { get; set; }

        // Chỉ số nước
        public int WaterOld { get; set; }
        public int WaterNew { get; set; }
        public decimal WaterPrice { get; set; }

        // Tiền phòng (Snapshot)
        public decimal RoomPrice { get; set; }

        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // 'ChuaThu', 'DaThu'

        // Display
        public int ElecUsage => ElecNew - ElecOld;
        public int WaterUsage => WaterNew - WaterOld;
    }
}