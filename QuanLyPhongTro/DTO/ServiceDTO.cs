namespace QuanLyPhongTro.DTO
{
    public class ServiceDTO
    {
        public int ServiceId { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public string ServiceType { get; set; }  // 'ChiSo', 'CoDinh', 'SoLuong'
        public string UnitName { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; }
        public bool IsFree { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public string Note { get; set; }

        // Display
        public string StatusDisplay => IsFree ? "Miễn phí" : $"{UnitPrice:N0}/{UnitName}";
        public string TypeDisplay => ServiceType == "ChiSo" ? "Theo chỉ số" : 
                                     (ServiceType == "CoDinh" ? "Cố định" : "Theo số lượng");
    }

    public class RoomServiceDTO
    {
        public int RoomServiceId { get; set; }
        public int RoomId { get; set; }
        public int ServiceId { get; set; }
        public bool IsEnabled { get; set; }
        public bool? IsFreeOverride { get; set; }
        public decimal? CustomPrice { get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; }

        // Display (từ JOIN)
        public string ServiceName { get; set; }
        public string ServiceCode { get; set; }
        public decimal EffectivePrice { get; set; }
        public bool EffectiveIsFree { get; set; }
    }
}
