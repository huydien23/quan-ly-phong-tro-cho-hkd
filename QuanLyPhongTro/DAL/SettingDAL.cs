using System;

namespace QuanLyPhongTro.DAL
{
    public class SettingDAL
    {
        public decimal GetValue(string key)
        {
            string sql = $"SELECT SettingValue FROM Settings WHERE SettingKey = '{key}'";
            var rs = DatabaseHelper.ExecuteScalar(sql);
            return rs != DBNull.Value ? Convert.ToDecimal(rs) : 0;
        }

        public void UpdateValue(string key, decimal value)
        {
            string sql = $"UPDATE Settings SET SettingValue = {value} WHERE SettingKey = '{key}'";
            DatabaseHelper.ExecuteNonQuery(sql);
        }
    }
}