using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.DAL
{
    public class RoomDAL : BaseRepository
    {
        #region Queries

        public List<RoomDTO> GetAllRooms()
        {
            const string sql = @"
                SELECT r.Id, r.RoomName, r.Status, r.RoomTypeId, 
                       t.TypeName, t.Price, t.Area 
                FROM Rooms r
                LEFT JOIN RoomTypes t ON r.RoomTypeId = t.Id
                ORDER BY r.RoomName";

            var dt = DatabaseHelper.ExecuteQuery(sql);
            return MapToList<RoomDTO>(dt);
        }

        public RoomDTO GetById(int id)
        {
            const string sql = @"
                SELECT r.Id, r.RoomName, r.Status, r.RoomTypeId, 
                       t.TypeName, t.Price, t.Area 
                FROM Rooms r
                LEFT JOIN RoomTypes t ON r.RoomTypeId = t.Id
                WHERE r.Id = @Id";

            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@Id", id));
            return dt.Rows.Count > 0 ? MapToObject<RoomDTO>(dt.Rows[0]) : null;
        }

        public DataTable GetRoomsWithTenant()
        {
            const string sql = @"
                SELECT r.*, t.Price, cu.FullName AS TenantName
                FROM Rooms r
                LEFT JOIN RoomTypes t ON r.RoomTypeId = t.Id
                LEFT JOIN Contracts c ON r.Id = c.RoomId AND c.IsActive = 1
                LEFT JOIN Customers cu ON c.CustomerId = cu.Id
                ORDER BY r.RoomName";

            return DatabaseHelper.ExecuteQuery(sql);
        }

        public bool CheckRoomNameExists(string roomName, int? excludeId = null)
        {
            var sql = excludeId.HasValue
                ? "SELECT COUNT(*) FROM Rooms WHERE RoomName = @Name AND Id <> @ExcludeId"
                : "SELECT COUNT(*) FROM Rooms WHERE RoomName = @Name";

            var parameters = excludeId.HasValue
                ? new[] { Param("@Name", roomName), Param("@ExcludeId", excludeId.Value) }
                : new[] { Param("@Name", roomName) };

            var count = DatabaseHelper.ExecuteScalar(sql, parameters);
            return count != null && (int)count > 0;
        }

        #endregion

        #region Commands

        public int Insert(RoomDTO room)
        {
            const string sql = @"
                INSERT INTO Rooms (RoomName, RoomTypeId, Status) 
                VALUES (@RoomName, @RoomTypeId, @Status);
                SELECT SCOPE_IDENTITY();";

            var result = DatabaseHelper.ExecuteScalar(sql,
                Param("@RoomName", room.RoomName),
                Param("@RoomTypeId", room.RoomTypeId),
                Param("@Status", room.Status ?? "Trong"));

            return result != null ? System.Convert.ToInt32(result) : 0;
        }

        public bool Update(RoomDTO room)
        {
            const string sql = @"
                UPDATE Rooms 
                SET RoomName = @RoomName, 
                    RoomTypeId = @RoomTypeId, 
                    Status = @Status 
                WHERE Id = @Id";

            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Id", room.Id),
                Param("@RoomName", room.RoomName),
                Param("@RoomTypeId", room.RoomTypeId),
                Param("@Status", room.Status)) > 0;
        }

        public bool UpdateStatus(int roomId, string status)
        {
            const string sql = "UPDATE Rooms SET Status = @Status WHERE Id = @Id";
            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Id", roomId),
                Param("@Status", status)) > 0;
        }

        public bool Delete(int id)
        {
            const string sql = "DELETE FROM Rooms WHERE Id = @Id";
            return DatabaseHelper.ExecuteNonQuery(sql, Param("@Id", id)) > 0;
        }

        #endregion

        #region Room Types

        public DataTable GetRoomTypes()
        {
            return DatabaseHelper.ExecuteQuery("SELECT * FROM RoomTypes ORDER BY TypeName");
        }

        public int InsertRoomType(string typeName, decimal price, double area)
        {
            const string sql = @"
                INSERT INTO RoomTypes (TypeName, Price, Area) 
                VALUES (@TypeName, @Price, @Area);
                SELECT SCOPE_IDENTITY();";

            var result = DatabaseHelper.ExecuteScalar(sql,
                Param("@TypeName", typeName),
                Param("@Price", price),
                Param("@Area", area));

            return result != null ? System.Convert.ToInt32(result) : 0;
        }

        #endregion
    }
}