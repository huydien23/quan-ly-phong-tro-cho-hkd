using System.Collections.Generic;
using System.Data;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.DAL
{
    public class ServiceDAL : BaseRepository
    {
        #region Services

        public List<ServiceDTO> GetAll(bool? activeOnly = true)
        {
            var sql = activeOnly == true
                ? "SELECT * FROM Services WHERE IsActive = 1 ORDER BY SortOrder"
                : "SELECT * FROM Services ORDER BY SortOrder";
            
            var dt = DatabaseHelper.ExecuteQuery(sql);
            return MapToList<ServiceDTO>(dt);
        }

        public ServiceDTO GetById(int id)
        {
            const string sql = "SELECT * FROM Services WHERE ServiceId = @Id";
            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@Id", id));
            return dt.Rows.Count > 0 ? MapToObject<ServiceDTO>(dt.Rows[0]) : null;
        }

        public ServiceDTO GetByCode(string code)
        {
            const string sql = "SELECT * FROM Services WHERE ServiceCode = @Code";
            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@Code", code));
            return dt.Rows.Count > 0 ? MapToObject<ServiceDTO>(dt.Rows[0]) : null;
        }

        public bool Update(ServiceDTO service)
        {
            const string sql = @"
                UPDATE Services SET 
                    ServiceName = @Name, UnitPrice = @Price, 
                    IsActive = @IsActive, IsFree = @IsFree, Note = @Note
                WHERE ServiceId = @Id";

            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Id", service.ServiceId),
                Param("@Name", service.ServiceName),
                Param("@Price", service.UnitPrice),
                Param("@IsActive", service.IsActive),
                Param("@IsFree", service.IsFree),
                Param("@Note", service.Note)) > 0;
        }

        public bool UpdatePrice(string serviceCode, decimal price, bool isFree)
        {
            const string sql = "UPDATE Services SET UnitPrice = @Price, IsFree = @IsFree WHERE ServiceCode = @Code";
            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Code", serviceCode),
                Param("@Price", price),
                Param("@IsFree", isFree)) > 0;
        }

        #endregion

        #region Room Services

        public List<RoomServiceDTO> GetRoomServices(int roomId)
        {
            const string sql = @"
                SELECT rs.*, s.ServiceName, s.ServiceCode, s.UnitPrice, s.IsFree AS DefaultIsFree
                FROM RoomServices rs
                INNER JOIN Services s ON rs.ServiceId = s.ServiceId
                WHERE rs.RoomId = @RoomId";

            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@RoomId", roomId));
            return MapToList<RoomServiceDTO>(dt);
        }

        public bool SaveRoomService(RoomServiceDTO rs)
        {
            const string sql = @"
                IF EXISTS (SELECT 1 FROM RoomServices WHERE RoomId = @RoomId AND ServiceId = @ServiceId)
                    UPDATE RoomServices SET IsEnabled = @IsEnabled, IsFreeOverride = @IsFreeOverride, 
                           CustomPrice = @CustomPrice, Quantity = @Quantity, Note = @Note
                    WHERE RoomId = @RoomId AND ServiceId = @ServiceId
                ELSE
                    INSERT INTO RoomServices (RoomId, ServiceId, IsEnabled, IsFreeOverride, CustomPrice, Quantity, Note)
                    VALUES (@RoomId, @ServiceId, @IsEnabled, @IsFreeOverride, @CustomPrice, @Quantity, @Note)";

            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@RoomId", rs.RoomId),
                Param("@ServiceId", rs.ServiceId),
                Param("@IsEnabled", rs.IsEnabled),
                Param("@IsFreeOverride", rs.IsFreeOverride),
                Param("@CustomPrice", rs.CustomPrice),
                Param("@Quantity", rs.Quantity),
                Param("@Note", rs.Note)) > 0;
        }

        #endregion

        #region Helpers - Lấy giá dịch vụ

        public decimal GetElectricPrice() => GetServicePrice("DIEN");
        public decimal GetWaterPrice() => GetServicePrice("NUOC");
        public decimal GetWifiPrice() => GetServicePrice("WIFI");
        public decimal GetTrashPrice() => GetServicePrice("RAC");

        public bool IsServiceFree(string code)
        {
            var service = GetByCode(code);
            return service?.IsFree ?? false;
        }

        private decimal GetServicePrice(string code)
        {
            var service = GetByCode(code);
            return service?.UnitPrice ?? 0;
        }

        #endregion
    }
}
