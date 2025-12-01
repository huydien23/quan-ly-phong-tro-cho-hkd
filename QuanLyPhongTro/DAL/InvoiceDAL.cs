using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.DAL
{
    public class InvoiceDAL : BaseRepository
    {
        #region Queries

        public List<InvoiceDTO> GetAll()
        {
            const string sql = @"
                SELECT i.*, r.RoomName, cu.FullName AS CustomerName
                FROM Invoices i
                INNER JOIN Contracts c ON i.ContractId = c.Id
                INNER JOIN Rooms r ON c.RoomId = r.Id
                INNER JOIN Customers cu ON c.CustomerId = cu.Id
                ORDER BY i.Year DESC, i.Month DESC";

            return MapToList<InvoiceDTO>(DatabaseHelper.ExecuteQuery(sql));
        }

        public InvoiceDTO GetById(int id)
        {
            const string sql = @"
                SELECT i.*, r.RoomName, cu.FullName AS CustomerName
                FROM Invoices i
                INNER JOIN Contracts c ON i.ContractId = c.Id
                INNER JOIN Rooms r ON c.RoomId = r.Id
                INNER JOIN Customers cu ON c.CustomerId = cu.Id
                WHERE i.Id = @Id";

            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@Id", id));
            return dt.Rows.Count > 0 ? MapToObject<InvoiceDTO>(dt.Rows[0]) : null;
        }

        public DataTable GetUnpaidInvoices()
        {
            const string sql = @"
                SELECT i.Id, r.RoomName, i.Month, i.Year, i.TotalAmount, i.Status 
                FROM Invoices i
                INNER JOIN Contracts c ON i.ContractId = c.Id
                INNER JOIN Rooms r ON c.RoomId = r.Id
                WHERE i.Status = 'ChuaThanhToan'
                ORDER BY i.Year DESC, i.Month DESC";

            return DatabaseHelper.ExecuteQuery(sql);
        }

        public DataTable GetInvoicesByRoom(int roomId)
        {
            const string sql = @"
                SELECT i.* 
                FROM Invoices i
                INNER JOIN Contracts c ON i.ContractId = c.Id
                WHERE c.RoomId = @RoomId
                ORDER BY i.Year DESC, i.Month DESC";

            return DatabaseHelper.ExecuteQuery(sql, Param("@RoomId", roomId));
        }

        public InvoiceDTO GetLastInvoiceByRoom(int roomId)
        {
            const string sql = @"
                SELECT TOP 1 i.* 
                FROM Invoices i
                INNER JOIN Contracts c ON i.ContractId = c.Id
                WHERE c.RoomId = @RoomId
                ORDER BY i.Id DESC";

            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@RoomId", roomId));
            return dt.Rows.Count > 0 ? MapToObject<InvoiceDTO>(dt.Rows[0]) : null;
        }

        public bool InvoiceExistsForMonth(int contractId, int month, int year)
        {
            const string sql = @"
                SELECT COUNT(*) FROM Invoices 
                WHERE ContractId = @ContractId AND Month = @Month AND Year = @Year";

            var count = DatabaseHelper.ExecuteScalar(sql,
                Param("@ContractId", contractId),
                Param("@Month", month),
                Param("@Year", year));

            return count != null && (int)count > 0;
        }

        #endregion

        #region Statistics

        public decimal GetTotalRevenueByYear(int year)
        {
            const string sql = "SELECT ISNULL(SUM(TotalAmount), 0) FROM Invoices WHERE Year = @Year";
            var result = DatabaseHelper.ExecuteScalar(sql, Param("@Year", year));
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        public decimal GetTotalRevenueByMonth(int month, int year)
        {
            const string sql = @"
                SELECT ISNULL(SUM(TotalAmount), 0) FROM Invoices 
                WHERE Month = @Month AND Year = @Year";

            var result = DatabaseHelper.ExecuteScalar(sql,
                Param("@Month", month),
                Param("@Year", year));

            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        public decimal GetTotalPaidByYear(int year)
        {
            const string sql = @"
                SELECT ISNULL(SUM(TotalAmount), 0) FROM Invoices 
                WHERE Year = @Year AND Status = 'DaThanhToan'";

            var result = DatabaseHelper.ExecuteScalar(sql, Param("@Year", year));
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        public int GetUnpaidCount()
        {
            const string sql = "SELECT COUNT(*) FROM Invoices WHERE Status = 'ChuaThanhToan'";
            var result = DatabaseHelper.ExecuteScalar(sql);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public DataTable GetMonthlyRevenueByYear(int year)
        {
            const string sql = @"
                SELECT Month, SUM(TotalAmount) AS Revenue
                FROM Invoices
                WHERE Year = @Year
                GROUP BY Month
                ORDER BY Month";

            return DatabaseHelper.ExecuteQuery(sql, Param("@Year", year));
        }

        #endregion

        #region Commands

        public int Insert(InvoiceDTO invoice)
        {
            const string sql = @"
                INSERT INTO Invoices (ContractId, Month, Year, ElecNew, ElecOld, ElecPrice, 
                                      WaterNew, WaterOld, WaterPrice, RoomPrice, TotalAmount, Status)
                VALUES (@ContractId, @Month, @Year, @ElecNew, @ElecOld, @ElecPrice,
                        @WaterNew, @WaterOld, @WaterPrice, @RoomPrice, @TotalAmount, @Status);
                SELECT SCOPE_IDENTITY();";

            var result = DatabaseHelper.ExecuteScalar(sql,
                Param("@ContractId", invoice.ContractId),
                Param("@Month", invoice.Month),
                Param("@Year", invoice.Year),
                Param("@ElecNew", invoice.ElecNew),
                Param("@ElecOld", invoice.ElecOld),
                Param("@ElecPrice", invoice.ElecPrice),
                Param("@WaterNew", invoice.WaterNew),
                Param("@WaterOld", invoice.WaterOld),
                Param("@WaterPrice", invoice.WaterPrice),
                Param("@RoomPrice", invoice.RoomPrice),
                Param("@TotalAmount", invoice.TotalAmount),
                Param("@Status", invoice.Status ?? "ChuaThanhToan"));

            return result != null ? Convert.ToInt32(result) : 0;
        }

        public bool UpdateStatus(int id, string status)
        {
            const string sql = "UPDATE Invoices SET Status = @Status WHERE Id = @Id";
            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Id", id),
                Param("@Status", status)) > 0;
        }

        public bool MarkAsPaid(int id)
        {
            return UpdateStatus(id, "DaThanhToan");
        }

        public bool Delete(int id)
        {
            const string sql = "DELETE FROM Invoices WHERE Id = @Id";
            return DatabaseHelper.ExecuteNonQuery(sql, Param("@Id", id)) > 0;
        }

        #endregion
    }
}