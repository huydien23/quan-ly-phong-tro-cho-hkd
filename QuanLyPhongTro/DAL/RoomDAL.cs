using System.Collections.Generic;
using QuanLyPhongTro.DTO;
using QuanLyPhongTro.Core; // Để dùng DataHelper.ToList

namespace QuanLyPhongTro.DAL
{
    public class RoomDAL
    {
        public List<RoomDTO> GetAllRooms()
        {
            // JOIN bảng để lấy luôn giá và tên loại phòng
            string sql = @"
                SELECT r.Id, r.RoomName, r.Status, r.RoomTypeId, 
                       t.TypeName, t.Price, t.Area 
                FROM Rooms r
                JOIN RoomTypes t ON r.RoomTypeId = t.Id";

            var dt = DatabaseHelper.ExecuteQuery(sql);
            return dt.ToList<RoomDTO>(); // Dùng Extension Method biến DataTable thành List
        }

        public bool InsertRoom(string name, int typeId)
        {
            string sql = $"INSERT INTO Rooms (RoomName, RoomTypeId, Status) VALUES (N'{name}', {typeId}, 'Trong')";
            return DatabaseHelper.ExecuteNonQuery(sql) > 0;
        }

        public void UpdateStatus(int roomId, string status)
        {
            string sql = $"UPDATE Rooms SET Status = '{status}' WHERE Id = {roomId}";
            DatabaseHelper.ExecuteNonQuery(sql);
        }
    }
}