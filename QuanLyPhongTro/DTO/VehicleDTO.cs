using System;

namespace QuanLyPhongTro.DTO
{
    public class VehicleDTO
    {
        public int VehicleId { get; set; }
        public int CustomerId { get; set; }
        public string VehicleType { get; set; }  // 'XeMay', 'XeDap', 'OTo'
        public string LicensePlate { get; set; }
        public string Brand { get; set; }
        public string Color { get; set; }
        public decimal MonthlyFee { get; set; }
        public bool IsFree { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }

        // Display (từ JOIN)
        public string CustomerName { get; set; }
        public string RoomName { get; set; }

        public string VehicleTypeDisplay
        {
            get
            {
                switch (VehicleType)
                {
                    case "XeMay": return "Xe máy";
                    case "XeDap": return "Xe đạp";
                    case "OTo": return "Ô tô";
                    default: return VehicleType;
                }
            }
        }

        public string FeeDisplay => IsFree ? "Miễn phí" : $"{MonthlyFee:N0}/tháng";
    }
}
