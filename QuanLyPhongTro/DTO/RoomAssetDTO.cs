using System;

namespace QuanLyPhongTro.DTO
{
    public class RoomAssetDTO
    {
        public int AssetId { get; set; }
        public int RoomId { get; set; }
        public string AssetName { get; set; }
        public string AssetType { get; set; }  // 'DieuHoa', 'NongLanh', 'Giuong'...
        public string Brand { get; set; }
        public int Quantity { get; set; }
        public string Condition { get; set; }  // 'Moi', 'Tot', 'Cu', 'Hong'
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public string Note { get; set; }

        // Display
        public string RoomName { get; set; }

        public string ConditionDisplay
        {
            get
            {
                switch (Condition)
                {
                    case "Moi": return "Mới";
                    case "Tot": return "Tốt";
                    case "Cu": return "Cũ";
                    case "Hong": return "Hỏng";
                    default: return Condition;
                }
            }
        }

        public string AssetTypeDisplay
        {
            get
            {
                switch (AssetType)
                {
                    case "DieuHoa": return "Điều hòa";
                    case "NongLanh": return "Bình nóng lạnh";
                    case "Giuong": return "Giường";
                    case "TuAo": return "Tủ quần áo";
                    case "Ban": return "Bàn";
                    case "Ghe": return "Ghế";
                    case "TuLanh": return "Tủ lạnh";
                    case "MayGiat": return "Máy giặt";
                    default: return AssetType;
                }
            }
        }
    }
}
