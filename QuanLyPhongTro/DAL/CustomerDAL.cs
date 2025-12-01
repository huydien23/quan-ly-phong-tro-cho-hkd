using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.DAL
{
    public class CustomerDAL : BaseRepository
    {
        #region Queries

        public List<CustomerDTO> GetAll()
        {
            const string sql = "SELECT * FROM Customers ORDER BY FullName";
            var dt = DatabaseHelper.ExecuteQuery(sql);
            return MapToList<CustomerDTO>(dt);
        }

        public List<CustomerDTO> Search(string keyword)
        {
            const string sql = @"
                SELECT * FROM Customers 
                WHERE FullName LIKE @Keyword OR Phone LIKE @Keyword OR CCCD LIKE @Keyword
                ORDER BY FullName";

            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@Keyword", $"%{keyword}%"));
            return MapToList<CustomerDTO>(dt);
        }

        public CustomerDTO GetById(int id)
        {
            const string sql = "SELECT * FROM Customers WHERE CustomerId = @Id";
            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@Id", id));
            return dt.Rows.Count > 0 ? MapToObject<CustomerDTO>(dt.Rows[0]) : null;
        }

        public bool CheckCCCDExists(string cccd, int? excludeId = null)
        {
            var sql = excludeId.HasValue
                ? "SELECT COUNT(*) FROM Customers WHERE CCCD = @CCCD AND CustomerId <> @ExcludeId"
                : "SELECT COUNT(*) FROM Customers WHERE CCCD = @CCCD";

            var parameters = excludeId.HasValue
                ? new[] { Param("@CCCD", cccd), Param("@ExcludeId", excludeId.Value) }
                : new[] { Param("@CCCD", cccd) };

            var count = DatabaseHelper.ExecuteScalar(sql, parameters);
            return count != null && Convert.ToInt32(count) > 0;
        }

        #endregion

        #region Commands

        public int Insert(CustomerDTO cus)
        {
            const string sql = @"
                INSERT INTO Customers (FullName, Phone, CCCD, Email, DateOfBirth, Gender, Job, Workplace, Address, EmergencyContact, EmergencyPhone, Note)
                VALUES (@FullName, @Phone, @CCCD, @Email, @DOB, @Gender, @Job, @Workplace, @Address, @EmergencyContact, @EmergencyPhone, @Note);
                SELECT SCOPE_IDENTITY();";

            var result = DatabaseHelper.ExecuteScalar(sql,
                Param("@FullName", cus.FullName),
                Param("@Phone", cus.Phone),
                Param("@CCCD", cus.CCCD),
                Param("@Email", cus.Email),
                Param("@DOB", cus.DateOfBirth),
                Param("@Gender", cus.Gender),
                Param("@Job", cus.Job),
                Param("@Workplace", cus.Workplace),
                Param("@Address", cus.Address),
                Param("@EmergencyContact", cus.EmergencyContact),
                Param("@EmergencyPhone", cus.EmergencyPhone),
                Param("@Note", cus.Note));

            return result != null ? Convert.ToInt32(result) : 0;
        }

        public bool Update(CustomerDTO cus)
        {
            const string sql = @"
                UPDATE Customers SET 
                    FullName = @FullName, Phone = @Phone, CCCD = @CCCD, Email = @Email,
                    DateOfBirth = @DOB, Gender = @Gender, Job = @Job, Workplace = @Workplace,
                    Address = @Address, EmergencyContact = @EmergencyContact, 
                    EmergencyPhone = @EmergencyPhone, Note = @Note
                WHERE CustomerId = @Id";

            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Id", cus.Id),
                Param("@FullName", cus.FullName),
                Param("@Phone", cus.Phone),
                Param("@CCCD", cus.CCCD),
                Param("@Email", cus.Email),
                Param("@DOB", cus.DateOfBirth),
                Param("@Gender", cus.Gender),
                Param("@Job", cus.Job),
                Param("@Workplace", cus.Workplace),
                Param("@Address", cus.Address),
                Param("@EmergencyContact", cus.EmergencyContact),
                Param("@EmergencyPhone", cus.EmergencyPhone),
                Param("@Note", cus.Note)) > 0;
        }

        public bool Delete(int id)
        {
            const string sql = "DELETE FROM Customers WHERE CustomerId = @Id";
            return DatabaseHelper.ExecuteNonQuery(sql, Param("@Id", id)) > 0;
        }

        #endregion
    }
}