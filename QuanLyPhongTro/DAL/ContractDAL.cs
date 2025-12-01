using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.DAL
{
    public class ContractDAL : BaseRepository
    {
        #region Queries

        public List<ContractDTO> GetAll()
        {
            const string sql = @"
                SELECT c.*, r.RoomName, cu.FullName AS CustomerName
                FROM Contracts c
                INNER JOIN Rooms r ON c.RoomId = r.Id
                INNER JOIN Customers cu ON c.CustomerId = cu.Id
                ORDER BY c.StartDate DESC";

            return MapToList<ContractDTO>(DatabaseHelper.ExecuteQuery(sql));
        }

        public ContractDTO GetById(int id)
        {
            const string sql = @"
                SELECT c.*, r.RoomName, cu.FullName AS CustomerName
                FROM Contracts c
                INNER JOIN Rooms r ON c.RoomId = r.Id
                INNER JOIN Customers cu ON c.CustomerId = cu.Id
                WHERE c.Id = @Id";

            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@Id", id));
            return dt.Rows.Count > 0 ? MapToObject<ContractDTO>(dt.Rows[0]) : null;
        }

        public ContractDTO GetActiveByRoomId(int roomId)
        {
            const string sql = @"
                SELECT c.*, r.RoomName, cu.FullName AS CustomerName, 
                       (SELECT Price FROM RoomTypes WHERE Id = r.RoomTypeId) AS RoomPrice
                FROM Contracts c
                INNER JOIN Rooms r ON c.RoomId = r.Id
                INNER JOIN Customers cu ON c.CustomerId = cu.Id
                WHERE c.RoomId = @RoomId AND c.IsActive = 1
                ORDER BY c.Id DESC";

            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@RoomId", roomId));
            return dt.Rows.Count > 0 ? MapToObject<ContractDTO>(dt.Rows[0]) : null;
        }

        public DataTable GetActiveContracts()
        {
            const string sql = @"
                SELECT c.Id, r.RoomName, cu.FullName AS CustomerName, c.StartDate, 
                       c.EndDate, c.Deposit, c.MonthlyRent
                FROM Contracts c
                INNER JOIN Rooms r ON c.RoomId = r.Id
                INNER JOIN Customers cu ON c.CustomerId = cu.Id
                WHERE c.IsActive = 1
                ORDER BY r.RoomName";

            return DatabaseHelper.ExecuteQuery(sql);
        }

        public bool HasActiveContract(int roomId)
        {
            const string sql = "SELECT COUNT(*) FROM Contracts WHERE RoomId = @RoomId AND IsActive = 1";
            var count = DatabaseHelper.ExecuteScalar(sql, Param("@RoomId", roomId));
            return count != null && (int)count > 0;
        }

        #endregion

        #region Commands

        public int Insert(ContractDTO contract)
        {
            const string sql = @"
                INSERT INTO Contracts (RoomId, CustomerId, StartDate, EndDate, Deposit, MonthlyRent, IsActive, Note) 
                VALUES (@RoomId, @CustomerId, @StartDate, @EndDate, @Deposit, @MonthlyRent, @IsActive, @Note);
                SELECT SCOPE_IDENTITY();";

            var result = DatabaseHelper.ExecuteScalar(sql,
                Param("@RoomId", contract.RoomId),
                Param("@CustomerId", contract.CustomerId),
                Param("@StartDate", contract.StartDate),
                Param("@EndDate", contract.EndDate),
                Param("@Deposit", contract.Deposit),
                Param("@MonthlyRent", contract.MonthlyRent),
                Param("@IsActive", contract.IsActive),
                Param("@Note", contract.Note));

            return result != null ? Convert.ToInt32(result) : 0;
        }

        public int InsertWithTransaction(DatabaseHelper db, ContractDTO contract)
        {
            const string sql = @"
                INSERT INTO Contracts (RoomId, CustomerId, StartDate, EndDate, Deposit, MonthlyRent, IsActive, Note) 
                VALUES (@RoomId, @CustomerId, @StartDate, @EndDate, @Deposit, @MonthlyRent, @IsActive, @Note);
                SELECT SCOPE_IDENTITY();";

            var result = db.ExecuteScalarWithTransaction(sql,
                Param("@RoomId", contract.RoomId),
                Param("@CustomerId", contract.CustomerId),
                Param("@StartDate", contract.StartDate),
                Param("@EndDate", contract.EndDate),
                Param("@Deposit", contract.Deposit),
                Param("@MonthlyRent", contract.MonthlyRent),
                Param("@IsActive", contract.IsActive),
                Param("@Note", contract.Note));

            return result != null ? Convert.ToInt32(result) : 0;
        }

        public bool Update(ContractDTO contract)
        {
            const string sql = @"
                UPDATE Contracts 
                SET RoomId = @RoomId,
                    CustomerId = @CustomerId,
                    StartDate = @StartDate,
                    EndDate = @EndDate,
                    Deposit = @Deposit,
                    MonthlyRent = @MonthlyRent,
                    IsActive = @IsActive,
                    Note = @Note
                WHERE Id = @Id";

            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Id", contract.Id),
                Param("@RoomId", contract.RoomId),
                Param("@CustomerId", contract.CustomerId),
                Param("@StartDate", contract.StartDate),
                Param("@EndDate", contract.EndDate),
                Param("@Deposit", contract.Deposit),
                Param("@MonthlyRent", contract.MonthlyRent),
                Param("@IsActive", contract.IsActive),
                Param("@Note", contract.Note)) > 0;
        }

        public bool Deactivate(int id)
        {
            const string sql = "UPDATE Contracts SET IsActive = 0 WHERE Id = @Id";
            return DatabaseHelper.ExecuteNonQuery(sql, Param("@Id", id)) > 0;
        }

        public bool DeactivateWithTransaction(DatabaseHelper db, int id)
        {
            const string sql = "UPDATE Contracts SET IsActive = 0 WHERE Id = @Id";
            return db.ExecuteNonQueryWithTransaction(sql, Param("@Id", id)) > 0;
        }

        public bool Delete(int id)
        {
            const string sql = "DELETE FROM Contracts WHERE Id = @Id";
            return DatabaseHelper.ExecuteNonQuery(sql, Param("@Id", id)) > 0;
        }

        #endregion
    }
}