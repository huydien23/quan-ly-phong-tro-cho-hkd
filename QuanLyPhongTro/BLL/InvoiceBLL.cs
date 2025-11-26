using System;
using System.Data;
using QuanLyPhongTro.DAL;

namespace QuanLyPhongTro.BLL
{
    public class InvoiceBLL
    {
        // Lấy danh sách phòng cần ghi điện nước (chỉ lấy phòng đang thuê)
        public DataTable GetRoomsForRecording()
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT r.Id as RoomId, r.RoomName, 
                       (SELECT TOP 1 ElecNew FROM Invoices WHERE ContractId IN (SELECT Id FROM Contracts WHERE RoomId = r.Id) ORDER BY Id DESC) as ElecOld,
                       (SELECT TOP 1 WaterNew FROM Invoices WHERE ContractId IN (SELECT Id FROM Contracts WHERE RoomId = r.Id) ORDER BY Id DESC) as WaterOld
                FROM Rooms r 
                WHERE r.Status = 'DangThue'");
        }

        // Tính toán và Lưu hóa đơn
        public string CalculateAndSave(int roomId, int elecNew, int waterNew)
        {
            // 1. Lấy thông tin Hợp đồng & Giá hiện tại
            string sqlContract = $"SELECT TOP 1 Id, (SELECT Price FROM RoomTypes WHERE Id = (SELECT RoomTypeId FROM Rooms WHERE Id = {roomId})) as RoomPrice FROM Contracts WHERE RoomId = {roomId} AND IsActive = 1";
            DataTable dtContract = DatabaseHelper.ExecuteQuery(sqlContract);

            if (dtContract.Rows.Count == 0) return "Không tìm thấy hợp đồng!";

            int contractId = Convert.ToInt32(dtContract.Rows[0]["Id"]);
            decimal roomPrice = Convert.ToDecimal(dtContract.Rows[0]["RoomPrice"]);

            // 2. Lấy chỉ số cũ (Logic: Nếu chưa có hóa đơn nào thì lấy chỉ số đầu vào lúc tạo HĐ - Demo lấy bằng 0)
            // Trong thực tế bạn cần lưu chỉ số đầu vào trong bảng Contracts
            int elecOld = 0;
            int waterOld = 0;

            // 3. Lấy Đơn giá từ Settings (Bảng Cấu hình)
            decimal giaDien = GetSettingValue("GiaDien");
            decimal giaNuoc = GetSettingValue("GiaNuoc");

            // 4. Tính tiền
            decimal tienDien = (elecNew - elecOld) * giaDien;
            decimal tienNuoc = (waterNew - waterOld) * giaNuoc;
            decimal tongTien = roomPrice + tienDien + tienNuoc;

            // 5. Lưu xuống DB (Snapshot giá)
            string insertSql = string.Format(@"
                INSERT INTO Invoices (ContractId, Month, Year, ElecNew, ElecOld, ElecPrice, WaterNew, WaterOld, WaterPrice, RoomPrice, TotalAmount, Status)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, 'ChuaThanhToan')",
                contractId, DateTime.Now.Month, DateTime.Now.Year,
                elecNew, elecOld, giaDien,
                waterNew, waterOld, giaNuoc,
                roomPrice, tongTien
            );

            DatabaseHelper.ExecuteNonQuery(insertSql);
            return "Success";
        }

        private decimal GetSettingValue(string key)
        {
            var obj = DatabaseHelper.ExecuteScalar($"SELECT SettingValue FROM Settings WHERE SettingKey = '{key}'");
            return obj != null ? Convert.ToDecimal(obj) : 0;
        }

        // === LOGIC TÍNH THUẾ (Dùng cho Dashboard) ===
        public TaxResult CalculateTax(int year)
        {
            // Lấy tổng doanh thu năm
            string sql = $"SELECT SUM(TotalAmount) FROM Invoices WHERE Year = {year}";
            var obj = DatabaseHelper.ExecuteScalar(sql);
            decimal revenue = obj != DBNull.Value ? Convert.ToDecimal(obj) : 0;

            // Gọi logic tính thuế (Class TaxCalculator đã viết ở câu trả lời trước)
            // Bạn có thể nhúng class TaxCalculator vào đây luôn hoặc để riêng
            return new TaxCalculator().Calculate(revenue);
        }
    }

    // Copy lại Class TaxCalculator vào đây hoặc để file riêng
    public class TaxCalculator
    {
        // ... (Code logic tính thuế 100tr, 5% đã cung cấp ở câu trả lời trước) ...
        // Bạn cần paste lại đoạn code đó vào đây để code chạy được
        public TaxResult Calculate(decimal yearlyRevenue)
        {
            // ... Logic if else ...
            return new TaxResult(); // Demo return
        }
    }

    public class TaxResult
    {
        public decimal TotalTax { get; set; }
        // ... properties ...
    }
}