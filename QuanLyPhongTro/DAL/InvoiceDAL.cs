using System.Data;

namespace QuanLyPhongTro.DAL
{
    public class InvoiceDAL
    {
        public void AddInvoice(int contractId, int month, int year, int eNew, int eOld, decimal ePrice, int wNew, int wOld, decimal wPrice, decimal rPrice, decimal total)
        {
            string sql = string.Format(@"
                INSERT INTO Invoices (ContractId, Month, Year, ElecNew, ElecOld, ElecPrice, WaterNew, WaterOld, WaterPrice, RoomPrice, TotalAmount, Status)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, 'ChuaThanhToan')",
                contractId, month, year, eNew, eOld, ePrice, wNew, wOld, wPrice, rPrice, total
            );
            DatabaseHelper.ExecuteNonQuery(sql);
        }

        public DataTable GetUnpaidInvoices()
        {
            // Join nhiều bảng để lấy tên phòng hiển thị
            string sql = @"
                SELECT i.Id, r.RoomName, i.Month, i.Year, i.TotalAmount, i.Status 
                FROM Invoices i
                JOIN Contracts c ON i.ContractId = c.Id
                JOIN Rooms r ON c.RoomId = r.Id
                WHERE i.Status = 'ChuaThanhToan'";
            return DatabaseHelper.ExecuteQuery(sql);
        }

        public decimal GetTotalRevenueYear(int year)
        {
            string sql = $"SELECT SUM(TotalAmount) FROM Invoices WHERE Year = {year}";
            var result = DatabaseHelper.ExecuteScalar(sql);
            return result != System.DBNull.Value ? System.Convert.ToDecimal(result) : 0;
        }
    }
}