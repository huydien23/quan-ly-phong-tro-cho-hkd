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
                SELECT Id, Username, FullName, Role 
                FROM Users 
                WHERE Username = @Username AND PasswordHash = @PasswordHash";

            var dt = DatabaseHelper.ExecuteQuery(sql,
                Param("@Username", username),
                Param("@PasswordHash", passwordHash));

            return dt.Rows.Count > 0 ? MapToObject<UserDTO>(dt.Rows[0]) : null;
        }

        public UserDTO GetUserByUsername(string username)
        {
            const string sql = "SELECT Id, Username, FullName, Role FROM Users WHERE Username = @Username";
            var dt = DatabaseHelper.ExecuteQuery(sql, Param("@Username", username));
            return dt.Rows.Count > 0 ? MapToObject<UserDTO>(dt.Rows[0]) : null;
        }

        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            const string sql = "UPDATE Users SET PasswordHash = @PasswordHash WHERE Id = @Id";
            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Id", userId),
                Param("@PasswordHash", newPasswordHash)) > 0;
        }

        public bool UpdateProfile(int userId, string fullName)
        {
            const string sql = "UPDATE Users SET FullName = @FullName WHERE Id = @Id";
            return DatabaseHelper.ExecuteNonQuery(sql,
                Param("@Id", userId),
                Param("@FullName", fullName)) > 0;
        }
    }
}
