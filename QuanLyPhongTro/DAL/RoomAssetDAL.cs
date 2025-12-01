using System.Collections.Generic;
using System.Data;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.DAL
{
    public class RoomAssetDAL : BaseRepository
    {
        public List<RoomAssetDTO> GetByRoom(int roomId)
        {
            const string sql = "SELECT * FROM RoomAssets WHERE RoomId = @RoomId ORDER BY AssetName";
            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@RoomId", roomId));
            return MapToList<RoomAssetDTO>(dt);
        }

        public DataTable GetAllWithRoom()
        {
            const string sql = @"
                SELECT a.AssetId, r.RoomName, a.AssetName, a.Brand, a.Quantity,
                       CASE a.Condition WHEN 'Moi' THEN N'Mới' WHEN 'Tot' THEN N'Tốt' 
                            WHEN 'Cu' THEN N'Cũ' WHEN 'Hong' THEN N'Hỏng' ELSE a.Condition END AS ConditionDisplay,
                       a.Note, a.RoomId, a.Condition
                FROM RoomAssets a
                INNER JOIN Rooms r ON a.RoomId = r.RoomId
                ORDER BY r.RoomName, a.AssetName";
            
            return DatabaseHelper.ExecuteQuery(sql);
        }

        public RoomAssetDTO GetById(int id)
        {
            const string sql = @"
                SELECT a.*, r.RoomName 
                FROM RoomAssets a 
                INNER JOIN Rooms r ON a.RoomId = r.RoomId
                WHERE a.AssetId = @Id";

            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@Id", id));
            return dt.Rows.Count > 0 ? MapToObject<RoomAssetDTO>(dt.Rows[0]) : null;
        }

        public int Insert(RoomAssetDTO asset)
        {
            const string sql = @"
                INSERT INTO RoomAssets (RoomId, AssetName, AssetType, Brand, Quantity, Condition, PurchaseDate, PurchasePrice, Note)
                VALUES (@RoomId, @Name, @Type, @Brand, @Qty, @Condition, @PurchaseDate, @Price, @Note);
                SELECT SCOPE_IDENTITY();";

            var result = DatabaseHelper.ExecuteScalar(sql,
                Param("@RoomId", asset.RoomId),
                Param("@Name", asset.AssetName),
                Param("@Type", asset.AssetType),
                Param("@Brand", asset.Brand),
                Param("@Qty", asset.Quantity),
                Param("@Condition", asset.Condition),
                Param("@PurchaseDate", asset.PurchaseDate),
                Param("@Price", asset.PurchasePrice),
                Param("@Note", asset.Note));

            return result != null ? System.Convert.ToInt32(result) : 0;
        }

        public bool Update(RoomAssetDTO asset)
        {
            const string sql = @"
                UPDATE RoomAssets SET 
                    AssetName = @Name, AssetType = @Type, Brand = @Brand,
                    Quantity = @Qty, Condition = @Condition, 
                    PurchaseDate = @PurchaseDate, PurchasePrice = @Price, Note = @Note
                WHERE AssetId = @Id";

            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Id", asset.AssetId),
                Param("@Name", asset.AssetName),
                Param("@Type", asset.AssetType),
                Param("@Brand", asset.Brand),
                Param("@Qty", asset.Quantity),
                Param("@Condition", asset.Condition),
                Param("@PurchaseDate", asset.PurchaseDate),
                Param("@Price", asset.PurchasePrice),
                Param("@Note", asset.Note)) > 0;
        }

        public bool Delete(int id)
        {
            const string sql = "DELETE FROM RoomAssets WHERE AssetId = @Id";
            return DatabaseHelper.ExecuteNonQuery(sql, Param("@Id", id)) > 0;
        }
    }
}
