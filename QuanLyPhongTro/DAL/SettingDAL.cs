using System;
using System.Data;
using System.Data.SqlClient;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.DAL
{
    public class SettingDAL : BaseRepository
    {
        #region Queries

        public decimal GetValue(string key)
        {
            const string sql = "SELECT SettingValue FROM Settings WHERE SettingKey = @Key";
            var result = DatabaseHelper.ExecuteScalar(sql, Param("@Key", key));
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        public string GetStringValue(string key)
        {
            const string sql = "SELECT SettingValue FROM Settings WHERE SettingKey = @Key";
            var result = DatabaseHelper.ExecuteScalar(sql, Param("@Key", key));
            return result?.ToString();
        }

        public DataTable GetAll()
        {
            return DatabaseHelper.ExecuteQuery("SELECT * FROM Settings ORDER BY SettingKey");
        }

        #endregion

        #region Commands

        public bool UpdateValue(string key, decimal value)
        {
            const string sql = "UPDATE Settings SET SettingValue = @Value WHERE SettingKey = @Key";
            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Key", key),
                Param("@Value", value)) > 0;
        }

        public bool UpdateStringValue(string key, string value)
        {
            const string sql = "UPDATE Settings SET SettingValue = @Value WHERE SettingKey = @Key";
            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Key", key),
                Param("@Value", value)) > 0;
        }

        public bool InsertOrUpdate(string key, string value)
        {
            const string checkSql = "SELECT COUNT(*) FROM Settings WHERE SettingKey = @Key";
            var exists = (int)DatabaseHelper.ExecuteScalar(checkSql, Param("@Key", key)) > 0;

            if (exists)
            {
                return UpdateStringValue(key, value);
            }
            else
            {
                const string insertSql = "INSERT INTO Settings (SettingKey, SettingValue) VALUES (@Key, @Value)";
                return DatabaseHelper.ExecuteNonQuery(insertSql,
                    Param("@Key", key),
                    Param("@Value", value)) > 0;
            }
        }

        #endregion

        #region Common Settings

        public decimal GetElectricPrice() => GetValue("GiaDien");
        public decimal GetWaterPrice() => GetValue("GiaNuoc");
        public decimal GetInternetPrice() => GetValue("GiaInternet");
        public decimal GetGarbagePrice() => GetValue("GiaRac");

        public bool SetElectricPrice(decimal value) => UpdateValue("GiaDien", value);
        public bool SetWaterPrice(decimal value) => UpdateValue("GiaNuoc", value);
        public bool SetInternetPrice(decimal value) => UpdateValue("GiaInternet", value);
        public bool SetGarbagePrice(decimal value) => UpdateValue("GiaRac", value);

        #endregion
    }
}