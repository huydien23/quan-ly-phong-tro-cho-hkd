using System;
using System.Data.SqlClient;
using QuanLyPhongTro.DTO;

namespace QuanLyPhongTro.DAL
{
    public class AuthDAL : BaseRepository
    {
        public UserDTO GetUserByCredentials(string username, string passwordHash)
        {
            const string sql = @"
                SELECT Username, FullName, Role 
                FROM Users 
                WHERE Username = @Username AND PasswordHash = @PasswordHash";

            var dt = DatabaseHelper.ExecuteQuery(sql,
                Param("@Username", username),
                Param("@PasswordHash", passwordHash));

            return dt.Rows.Count > 0 ? MapToObject<UserDTO>(dt.Rows[0]) : null;
        }

        public UserDTO GetUserByUsername(string username)
        {
            const string sql = "SELECT Username, FullName, Role FROM Users WHERE Username = @Username";
            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@Username", username));
            return dt.Rows.Count > 0 ? MapToObject<UserDTO>(dt.Rows[0]) : null;
        }

        public bool UpdatePassword(string username, string newPasswordHash)
        {
            const string sql = "UPDATE Users SET PasswordHash = @PasswordHash WHERE Username = @Username";
            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Username", username),
                Param("@PasswordHash", newPasswordHash)) > 0;
        }

        public bool UpdateProfile(string username, string fullName)
        {
            const string sql = "UPDATE Users SET FullName = @FullName WHERE Username = @Username";
            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Username", username),
                Param("@FullName", fullName)) > 0;
        }
    }
}
