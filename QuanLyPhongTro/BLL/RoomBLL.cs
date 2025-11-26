using System.Data;
using QuanLyPhongTro.DAL;

namespace QuanLyPhongTro.BLL
{
    public class RoomBLL
    {
        public DataTable GetAllRooms()
        {
            return DatabaseHelper.ExecuteQuery("SELECT * FROM Rooms");
        }

        public string AddRoom(string name, int typeId)
        {
            // Validation
            if (string.IsNullOrEmpty(name)) return "Tên phòng không được để trống";

            // Check trùng tên
            DataTable dt = DatabaseHelper.ExecuteQuery($"SELECT * FROM Rooms WHERE RoomName = '{name}'");
            if (dt.Rows.Count > 0) return "Tên phòng đã tồn tại!";

            string sql = $"INSERT INTO Rooms (RoomName, RoomTypeId, Status) VALUES (N'{name}', {typeId}, 'Trong')";
            DatabaseHelper.ExecuteNonQuery(sql);
            return "Success";
        }
    }
}