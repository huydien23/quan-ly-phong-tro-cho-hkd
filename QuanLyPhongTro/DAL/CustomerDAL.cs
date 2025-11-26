using System.Collections.Generic;
using QuanLyPhongTro.DTO;
using QuanLyPhongTro.Core;

namespace QuanLyPhongTro.DAL
{
    public class CustomerDAL
    {
        public List<CustomerDTO> GetCustomers(string keyword = "")
        {
            string sql = "SELECT * FROM Customers";
            if (!string.IsNullOrEmpty(keyword))
            {
                sql += $" WHERE FullName LIKE N'%{keyword}%' OR IdentityCard LIKE '%{keyword}%'";
            }
            return DatabaseHelper.ExecuteQuery(sql).ToList<CustomerDTO>();
        }

        public bool AddCustomer(CustomerDTO cus)
        {
            string sql = $"INSERT INTO Customers (FullName, Phone, IdentityCard, Address) VALUES (N'{cus.FullName}', '{cus.Phone}', '{cus.IdentityCard}', N'{cus.Address}')";
            return DatabaseHelper.ExecuteNonQuery(sql) > 0;
        }
    }
}