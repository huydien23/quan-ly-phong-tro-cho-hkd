using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.DAL
{
    public class CustomerDAL : BaseRepository
    {
        #region Queries

        public List<CustomerDTO> GetCustomers(string keyword = "")
        {
            const string sql = @"
                SELECT * FROM Customers 
                WHERE (@Keyword IS NULL OR (@Keyword = '' OR FullName LIKE @Keyword OR Phone LIKE @Keyword OR IdentityCard LIKE @Keyword))
                ORDER BY FullName";

            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@Keyword", $"%{keyword}%"));
            return MapToList<CustomerDTO>(dt);
        }

        public bool AddCustomer(CustomerDTO cus)
        {
            const string sql = @"
                INSERT INTO Customers (FullName, Phone, IdentityCard, Address) 
                VALUES (@FullName, @Phone, @IdentityCard, @Address)";

            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@FullName", cus.FullName),
                Param("@Phone", cus.Phone),
                Param("@IdentityCard", cus.IdentityCard),
                Param("@Address", cus.Address)) > 0;
        }

        #endregion
    }
}