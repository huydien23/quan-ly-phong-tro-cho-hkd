using System;
using System.Collections.Generic;
using System.Data;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.DAL
{
    public class TransactionDAL : BaseRepository
    {
        #region Transactions

        public List<TransactionDTO> GetAll(DateTime? fromDate = null, DateTime? toDate = null, string type = null)
        {
            var sql = @"
                SELECT t.*, r.RoomName, cu.FullName AS CustomerName
                FROM Transactions t
                LEFT JOIN Rooms r ON t.RoomId = r.RoomId
                LEFT JOIN Customers cu ON t.CustomerId = cu.CustomerId
                WHERE 1=1";

            var parameters = new List<System.Data.SqlClient.SqlParameter>();

            if (fromDate.HasValue)
            {
                sql += " AND t.TransactionDate >= @FromDate";
                parameters.Add(Param("@FromDate", fromDate.Value));
            }
            if (toDate.HasValue)
            {
                sql += " AND t.TransactionDate <= @ToDate";
                parameters.Add(Param("@ToDate", toDate.Value));
            }
            if (!string.IsNullOrEmpty(type))
            {
                sql += " AND t.TransactionType = @Type";
                parameters.Add(Param("@Type", type));
            }

            sql += " ORDER BY t.TransactionDate DESC, t.TransactionId DESC";

            var dt = DatabaseHelper.ExecuteQuery(sql, parameters.ToArray());
            return MapToList<TransactionDTO>(dt);
        }

        public DataTable GetAllAsDataTable(int? month = null, int? year = null, string type = null)
        {
            var sql = @"
                SELECT t.TransactionId, t.TransactionDate,
                       CASE t.TransactionType WHEN 'Thu' THEN N'Thu' ELSE N'Chi' END AS TypeDisplay,
                       t.CategoryName, t.Amount, t.Description,
                       ISNULL(r.RoomName, '') AS RoomName,
                       t.PaymentMethod, t.ReceivedBy, t.TransactionType
                FROM Transactions t
                LEFT JOIN Rooms r ON t.RoomId = r.RoomId
                WHERE 1=1";

            var parameters = new List<System.Data.SqlClient.SqlParameter>();

            if (month.HasValue && year.HasValue)
            {
                sql += " AND MONTH(t.TransactionDate) = @Month AND YEAR(t.TransactionDate) = @Year";
                parameters.Add(Param("@Month", month.Value));
                parameters.Add(Param("@Year", year.Value));
            }
            if (!string.IsNullOrEmpty(type))
            {
                sql += " AND t.TransactionType = @Type";
                parameters.Add(Param("@Type", type));
            }

            sql += " ORDER BY t.TransactionDate DESC, t.TransactionId DESC";

            return DatabaseHelper.ExecuteQuery(sql, parameters.ToArray());
        }

        public TransactionDTO GetById(int id)
        {
            const string sql = @"
                SELECT t.*, r.RoomName, cu.FullName AS CustomerName
                FROM Transactions t
                LEFT JOIN Rooms r ON t.RoomId = r.RoomId
                LEFT JOIN Customers cu ON t.CustomerId = cu.CustomerId
                WHERE t.TransactionId = @Id";

            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@Id", id));
            return dt.Rows.Count > 0 ? MapToObject<TransactionDTO>(dt.Rows[0]) : null;
        }

        public int Insert(TransactionDTO trans)
        {
            const string sql = @"
                INSERT INTO Transactions 
                    (TransactionDate, TransactionType, CategoryCode, CategoryName, Amount, 
                     RoomId, CustomerId, InvoiceId, Description, PaymentMethod, ReceivedBy, Note, CreatedBy)
                VALUES 
                    (@Date, @Type, @CatCode, @CatName, @Amount, 
                     @RoomId, @CustomerId, @InvoiceId, @Desc, @PayMethod, @ReceivedBy, @Note, @CreatedBy);
                SELECT SCOPE_IDENTITY();";

            var result = DatabaseHelper.ExecuteScalar(sql,
                Param("@Date", trans.TransactionDate),
                Param("@Type", trans.TransactionType),
                Param("@CatCode", trans.CategoryCode),
                Param("@CatName", trans.CategoryName),
                Param("@Amount", trans.Amount),
                Param("@RoomId", trans.RoomId),
                Param("@CustomerId", trans.CustomerId),
                Param("@InvoiceId", trans.InvoiceId),
                Param("@Desc", trans.Description),
                Param("@PayMethod", trans.PaymentMethod),
                Param("@ReceivedBy", trans.ReceivedBy),
                Param("@Note", trans.Note),
                Param("@CreatedBy", trans.CreatedBy));

            return result != null ? Convert.ToInt32(result) : 0;
        }

        public bool Update(TransactionDTO trans)
        {
            const string sql = @"
                UPDATE Transactions SET 
                    TransactionDate = @Date, CategoryCode = @CatCode, CategoryName = @CatName,
                    Amount = @Amount, Description = @Desc, PaymentMethod = @PayMethod, 
                    ReceivedBy = @ReceivedBy, Note = @Note
                WHERE TransactionId = @Id";

            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Id", trans.TransactionId),
                Param("@Date", trans.TransactionDate),
                Param("@CatCode", trans.CategoryCode),
                Param("@CatName", trans.CategoryName),
                Param("@Amount", trans.Amount),
                Param("@Desc", trans.Description),
                Param("@PayMethod", trans.PaymentMethod),
                Param("@ReceivedBy", trans.ReceivedBy),
                Param("@Note", trans.Note)) > 0;
        }

        public bool Delete(int id)
        {
            const string sql = "DELETE FROM Transactions WHERE TransactionId = @Id";
            return DatabaseHelper.ExecuteNonQuery(sql, Param("@Id", id)) > 0;
        }

        #endregion

        #region Statistics

        public TransactionSummary GetSummary(int month, int year)
        {
            const string sql = @"
                SELECT 
                    ISNULL(SUM(CASE WHEN TransactionType = 'Thu' THEN Amount ELSE 0 END), 0) AS TotalIncome,
                    ISNULL(SUM(CASE WHEN TransactionType = 'Chi' THEN Amount ELSE 0 END), 0) AS TotalExpense,
                    COUNT(CASE WHEN TransactionType = 'Thu' THEN 1 END) AS IncomeCount,
                    COUNT(CASE WHEN TransactionType = 'Chi' THEN 1 END) AS ExpenseCount
                FROM Transactions
                WHERE MONTH(TransactionDate) = @Month AND YEAR(TransactionDate) = @Year";

            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@Month", month), Param("@Year", year));
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return new TransactionSummary
                {
                    TotalIncome = Convert.ToDecimal(row["TotalIncome"]),
                    TotalExpense = Convert.ToDecimal(row["TotalExpense"]),
                    IncomeCount = Convert.ToInt32(row["IncomeCount"]),
                    ExpenseCount = Convert.ToInt32(row["ExpenseCount"])
                };
            }
            return new TransactionSummary();
        }

        public decimal GetTotalIncomeByYear(int year)
        {
            const string sql = "SELECT ISNULL(SUM(Amount), 0) FROM Transactions WHERE TransactionType = 'Thu' AND YEAR(TransactionDate) = @Year";
            var result = DatabaseHelper.ExecuteScalar(sql, Param("@Year", year));
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        public decimal GetTotalExpenseByYear(int year)
        {
            const string sql = "SELECT ISNULL(SUM(Amount), 0) FROM Transactions WHERE TransactionType = 'Chi' AND YEAR(TransactionDate) = @Year";
            var result = DatabaseHelper.ExecuteScalar(sql, Param("@Year", year));
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        #endregion

        #region Categories

        public List<TransactionCategoryDTO> GetCategories(string type = null)
        {
            var sql = "SELECT * FROM TransactionCategories";
            if (!string.IsNullOrEmpty(type))
                sql += " WHERE TransactionType = @Type";
            sql += " ORDER BY SortOrder";

            var dt = string.IsNullOrEmpty(type)
                ? DatabaseHelper.ExecuteQuery(sql)
                : DatabaseHelper.ExecuteQuery(sql, Param("@Type", type));

            return MapToList<TransactionCategoryDTO>(dt);
        }

        #endregion
    }
}
