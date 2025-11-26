using System;
using System.Data; 
using QuanLyPhongTro.DAL;

namespace QuanLyPhongTro.BLL
{
    public class ContractBLL
    {
        public string CreateContract(int roomId, int customerId, DateTime startDate, decimal deposit)
        {
            // 1. Kiểm tra xem phòng có đang trống không (tránh trùng)
            string checkSql = $"SELECT Status FROM Rooms WHERE Id = {roomId}";
            var statusObj = GetScalarValue(checkSql); // Updated to use a helper method
            if (statusObj != null && statusObj.ToString() == "DangThue")
            {
                return "Phòng này vừa có người thuê rồi!";
            }

            // 2. Tạo Hợp Đồng
            string insertSql = string.Format(
                "INSERT INTO Contracts (RoomId, CustomerId, StartDate, Deposit, IsActive) VALUES ({0}, {1}, '{2}', {3}, 1)",
                roomId, customerId, startDate.ToString("yyyy-MM-dd"), deposit
            );

            // 3. Cập nhật trạng thái Phòng -> 'DangThue'
            string updateRoomSql = $"UPDATE Rooms SET Status = 'DangThue' WHERE Id = {roomId}";

            // Thực thi cả 2 lệnh (Nên dùng Transaction trong SQL, nhưng ở đây gọi code cho đơn giản)
            try
            {
                DatabaseHelper.ExecuteNonQuery(insertSql);
                DatabaseHelper.ExecuteNonQuery(updateRoomSql);
                return "Success";
            }
            catch (Exception ex)
            {
                return "Lỗi: " + ex.Message;
            }
        }

        private object GetScalarValue(string query)
        {
            DataTable result = DatabaseHelper.ExecuteQuery(query);
            if (result.Rows.Count > 0)
            {
                return result.Rows[0][0];
            }
            return null;
        }
    }
}