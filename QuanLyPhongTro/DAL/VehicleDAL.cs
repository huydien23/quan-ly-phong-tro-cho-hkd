using System.Collections.Generic;
using System.Data;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.DAL
{
    public class VehicleDAL : BaseRepository
    {
        public List<VehicleDTO> GetAll()
        {
            const string sql = @"
                SELECT v.*, cu.FullName AS CustomerName, r.RoomName
                FROM Vehicles v
                INNER JOIN Customers cu ON v.CustomerId = cu.CustomerId
                LEFT JOIN Contracts c ON cu.CustomerId = c.CustomerId AND c.IsActive = 1
                LEFT JOIN Rooms r ON c.RoomId = r.RoomId
                ORDER BY v.VehicleId DESC";

            var dt = DatabaseHelper.ExecuteQuery(sql);
            return MapToList<VehicleDTO>(dt);
        }

        public List<VehicleDTO> GetByCustomer(int customerId)
        {
            const string sql = "SELECT * FROM Vehicles WHERE CustomerId = @CustomerId ORDER BY VehicleId";
            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@CustomerId", customerId));
            return MapToList<VehicleDTO>(dt);
        }

        public VehicleDTO GetById(int id)
        {
            const string sql = @"
                SELECT v.*, cu.FullName AS CustomerName
                FROM Vehicles v
                INNER JOIN Customers cu ON v.CustomerId = cu.CustomerId
                WHERE v.VehicleId = @Id";

            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@Id", id));
            return dt.Rows.Count > 0 ? MapToObject<VehicleDTO>(dt.Rows[0]) : null;
        }

        public int Insert(VehicleDTO vehicle)
        {
            const string sql = @"
                INSERT INTO Vehicles (CustomerId, VehicleType, LicensePlate, Brand, Color, MonthlyFee, IsFree, Note)
                VALUES (@CustomerId, @VehicleType, @LicensePlate, @Brand, @Color, @MonthlyFee, @IsFree, @Note);
                SELECT SCOPE_IDENTITY();";

            var result = DatabaseHelper.ExecuteScalar(sql,
                Param("@CustomerId", vehicle.CustomerId),
                Param("@VehicleType", vehicle.VehicleType),
                Param("@LicensePlate", vehicle.LicensePlate),
                Param("@Brand", vehicle.Brand),
                Param("@Color", vehicle.Color),
                Param("@MonthlyFee", vehicle.MonthlyFee),
                Param("@IsFree", vehicle.IsFree),
                Param("@Note", vehicle.Note));

            return result != null ? System.Convert.ToInt32(result) : 0;
        }

        public bool Update(VehicleDTO vehicle)
        {
            const string sql = @"
                UPDATE Vehicles SET 
                    VehicleType = @VehicleType, LicensePlate = @LicensePlate,
                    Brand = @Brand, Color = @Color, MonthlyFee = @MonthlyFee, 
                    IsFree = @IsFree, Note = @Note
                WHERE VehicleId = @Id";

            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Id", vehicle.VehicleId),
                Param("@VehicleType", vehicle.VehicleType),
                Param("@LicensePlate", vehicle.LicensePlate),
                Param("@Brand", vehicle.Brand),
                Param("@Color", vehicle.Color),
                Param("@MonthlyFee", vehicle.MonthlyFee),
                Param("@IsFree", vehicle.IsFree),
                Param("@Note", vehicle.Note)) > 0;
        }

        public bool Delete(int id)
        {
            const string sql = "DELETE FROM Vehicles WHERE VehicleId = @Id";
            return DatabaseHelper.ExecuteNonQuery(sql, Param("@Id", id)) > 0;
        }

        public int CountByCustomer(int customerId)
        {
            const string sql = "SELECT COUNT(*) FROM Vehicles WHERE CustomerId = @CustomerId";
            var result = DatabaseHelper.ExecuteScalar(sql, Param("@CustomerId", customerId));
            return result != null ? System.Convert.ToInt32(result) : 0;
        }

        public DataTable GetVehiclesWithRoom()
        {
            const string sql = @"
                SELECT v.VehicleId, cu.FullName AS CustomerName, r.RoomName,
                       CASE v.VehicleType WHEN 'XeMay' THEN N'Xe máy' WHEN 'XeDap' THEN N'Xe đạp' WHEN 'OTo' THEN N'Ô tô' ELSE v.VehicleType END AS VehicleTypeName,
                       v.LicensePlate, v.Brand, v.Color, 
                       CASE WHEN v.IsFree = 1 THEN N'Miễn phí' ELSE FORMAT(v.MonthlyFee, 'N0') + N'/tháng' END AS FeeDisplay,
                       v.VehicleType, v.CustomerId, v.MonthlyFee, v.IsFree
                FROM Vehicles v
                INNER JOIN Customers cu ON v.CustomerId = cu.CustomerId
                LEFT JOIN Contracts c ON cu.CustomerId = c.CustomerId AND c.IsActive = 1
                LEFT JOIN Rooms r ON c.RoomId = r.RoomId
                ORDER BY r.RoomName, v.VehicleId";

            return DatabaseHelper.ExecuteQuery(sql);
        }
    }
}
