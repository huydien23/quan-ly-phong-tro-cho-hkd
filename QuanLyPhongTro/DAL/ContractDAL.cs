using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.DAL
{
    public class ContractDAL
    {
        public bool AddContract(ContractDTO contract)
        {
            string sql = string.Format(
                "INSERT INTO Contracts (RoomId, CustomerId, StartDate, Deposit, IsActive) VALUES ({0}, {1}, '{2}', {3}, 1)",
                contract.RoomId, contract.CustomerId, contract.StartDate.ToString("yyyy-MM-dd"), contract.Deposit
            );
            return DatabaseHelper.ExecuteNonQuery(sql) > 0;
        }

        public ContractDTO GetActiveContractByRoom(int roomId)
        {
            string sql = $"SELECT TOP 1 * FROM Contracts WHERE RoomId = {roomId} AND IsActive = 1 ORDER BY Id DESC";
            var dt = DatabaseHelper.ExecuteQuery(sql);
            if (dt.Rows.Count > 0)
            {
                // Map tay hoặc dùng Helper nếu muốn
                var row = dt.Rows[0];
                return new ContractDTO
                {
                    Id = (int)row["Id"],
                    RoomId = (int)row["RoomId"],
                    // ... map tiếp các trường khác
                };
            }
            return null;
        }
    }
}